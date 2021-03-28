precision highp float;

out vec4 fragColor;


// mat2x2 projJacob(mat3x3 surfJacob)//(vec2 ts, vec3 pos)
// {
//     //     mat4x2 * mat2x4 = mat2x2
//     // mat3x2 sj = surfaceJacob(ts);
//     // return mat4x2(sj[0], sj[1], sj[2], vec2(1, 1)) * projMat;
    
//     //     mat3x2 * mat2x3 = mat2x2
//     // return transpose(surfaceJacob(ts, pos)) * mat2x3(projMat);

// }

#define PROJJACOB(sj) (transpose(mat2x3(sj[1], sj[2])) * mat2x3(projMat))

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

int jumpFunc(int n) { return n*n*n; }
// n
// n*n, sqi(n)
// n*n*n
// (1 << n)
// sign(n) * (1 << (n - 1))
// sign(n) * int(pow(3., float(n-1)))
// sign(n) * (1 << (2*(n - 1)))
// n*(1 << (n-1))


const int MAX_N = 4;
#define BLUE_JUMP jumpFunc(4)  // MAX_N

void main( void )
{    
    ivec2 coord = ivec2(gl_FragCoord);
    vec2 fcoord = floor(gl_FragCoord.xy);


    if(frame == 0)
    {
        const vec2 initTS = vec2(0.5, 0.5);

        if(coord == ivec2(proj(surface(initTS))))
            fragColor = vec4(initTS, none, none);
        else
            fragColor = vec4(none);

        return;
    }

    bool onBlue = coord % BLUE_JUMP == ivec2(0);

    bool[] lastSettled = bool[2](false, false);
    float[] minForward = float[2](-1., -1.);


    fragColor = vec4(none);


    for(int n = 0; n <= MAX_N; n++)
        for(int i = 1; i < 8; i += 2 /* variable (1, 2) */)
        {
            ivec2 otherCoord = coord + jumpFunc(n) * relSquare(i);


            if(any(lessThan(otherCoord, ivec2(0))) || any(greaterThanEqual(otherCoord, resolution))) continue;


            vec4 otherVal = texelFetch(bufferA, otherCoord, 0);


            bool otherOnBlue = otherCoord % BLUE_JUMP == ivec2(0);

            // Look at the blue value of 'other' only if(otherOnBlue).
            for(int otherYellowBlue = 0; otherYellowBlue < 2 && (otherOnBlue || otherYellowBlue < 1); otherYellowBlue++)
            {
                vec2 ts = (otherYellowBlue == 0) ? otherVal.xy : otherVal.zw;

                if(!ISNONE(ts.x))
                {
                    vec3 surf = vec3(0);
                    vec2 surfProj = vec2(0);

                    mat3x3 sj = mat3x3(0);

                    // Newton's method iterations.
                    // Do more iterations when 'other' is blue.
                    for(int i = 0; i < (otherYellowBlue == 1 ? 5 : 2); i++) 
                    {
                        sj = transpose(surfaceAutoDiff(mat3x2(ts, 1, 0, 0, 1)));

                        surf = sj[0];
                        surfProj = proj(surf);

                        ts += (fcoord - surfProj) * inverse(PROJJACOB(sj));
                    }

                    if(any(isnan(ts)) || !inRange(ts)) continue;

                    surf = surface(ts);
                    surfProj = proj(surf);


                    bool newSettled = maxVal(abs(fcoord - surfProj)) <= 1.;//sq(1.);

                    if(newSettled)
                    {
                        float realForwardDir = dot(axes[2], surf);

                        // Update the blue value only if(onBlue).
                        for(int yellowBlue = 0; yellowBlue < 2 && (onBlue || yellowBlue < 1); yellowBlue++)
                        {
                            float forwardDir = realForwardDir * (yellowBlue == 0 ? 1. : -1.);

                            if(!lastSettled[yellowBlue] || forwardDir < minForward[yellowBlue])
                            {
                                if(yellowBlue == 0) fragColor.xy = ts;
                                else fragColor.zw = ts;

                                lastSettled[yellowBlue] = newSettled;
                                minForward[yellowBlue] = forwardDir;
                            }
                        }
                    }
                }
            }
        }

}



