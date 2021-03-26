precision highp float;

uniform int frame;

uniform sampler2D position3D;


out vec4 fragColor;


mat2x2 projJacob(vec2 ts, vec3 pos)
// TODO: define?
{
    //     mat4x2 * mat2x4 = mat2x2
    // mat3x2 sj = surfaceJacob(ts);
    // return mat4x2(sj[0], sj[1], sj[2], vec2(1, 1)) * projMat;
    
    //     mat3x2 * mat2x3 = mat2x2
    return transpose(surfaceJacob(ts, pos)) * mat2x3(projMat);
}

ivec2 relSquare(int i)
{
    return clamp(abs((ivec2(0, 2) + i) % 8 - 3) - 2, -1, 1);
}

ivec2 absSquare(int i, int n)
{
    if(n <= 0)
    {
        n++;  // weird
        return ivec2(0);
    }
    return clamp(abs((ivec2(0, 2*n) + i) % (8*n) - 3*n) - 2*n, -n, n);
}

ivec2 circle4(int i)
{
    i = (i%4 + 4) % 4;
    return ivec2(abs(2-i)-1, 1-abs(i-1));
}

vec2 multI(vec2 z)
{
    return vec2(-z.y, z.x);
}


void main( void )
{    
    ivec2 coord = ivec2(gl_FragCoord);
    vec2 fcoord = floor(gl_FragCoord.xy);

    if(frame == 0)
    {
        const vec2 initTS = vec2(0.5, 0.5);

        if(coord == ivec2(proj(surface(initTS))))
            fragColor = vec4(initTS, 0, ACTIVE);
        else
            fragColor = vec4(0, 0, 0, NONE);

        return;
    }

    const int searchSize = 2;

    bool lastSettled = false;
    float minForward = -1.;


    fragColor = vec4(0, 0, 0, NONE);

    // for(int x = max(0, coord.x - searchSize); x <= min(resolution.x - 1, coord.x + searchSize); x++)
    //     for(int y = max(0, coord.y - searchSize); y <= min(resolution.y - 1, coord.y + searchSize); y++)
    for(int n = 0; n <= 6; n++)
        // for(int i = 0; i < max(8*n, 1); i++)
        // for(int i = n; i < n + max(8*n, 1); i += max(2*n, 1))
        for(int i = 1; i < 8; i += 2)
        {
            // ivec2 otherCoord = ivec2(x, y);
            ivec2 otherCoord = coord + n*n*relSquare(i);

            if(any(lessThan(otherCoord, ivec2(0))) || any(greaterThanEqual(otherCoord, resolution))) continue;

            vec4 otherVal = texelFetch(bufferA, otherCoord, 0);

            if(MODE(otherVal) == ACTIVE)
            {
                vec2 oldTS = otherVal.xy;
                vec3 oldPosition = texelFetch(position3D, otherCoord, 0).xyz;
                

                mat2 pj = projJacob(oldTS, oldPosition);
                //if(abs(determinant(pj)) <= 0.001) continue;

                vec2 oldProj = proj(oldPosition);

                vec2 newTS = oldTS + (fcoord - oldProj) * inverse(pj);
                if(!inRange(newTS)) continue;
                //if(any(isnan(newTS)) || any(isinf(newTS))) continue;

                vec3 newSurf = surface(newTS);
                vec2 newProj = proj(newSurf);


                for(int i = 0; i < 1; i++)
                {
                    newTS += (fcoord - newProj) * inverse(projJacob(newTS, newSurf));

                    newSurf = surface(newTS);
                    newProj = proj(newSurf);
                }
                
                if(!inRange(newTS)) continue;



                float dist = sq(fcoord - newProj);

                bool newSettled = dist <= sq(0.5);

                if(newSettled)
                {
                    float forwardDir = dot(axes[2], newSurf);
                    if(!lastSettled || forwardDir < minForward)
                    {
                        fragColor = vec4(newTS, 0, ACTIVE);

                        lastSettled = newSettled;
                        minForward = forwardDir;
                    }
                }
            }
        }

}



