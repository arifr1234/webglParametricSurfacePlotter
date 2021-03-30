precision highp float;

uniform int START_N;
uniform int MAX_N;
uniform int BLUE_JUMP;

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

int lerp(int x, int y, int a) { return (1-a)*x + a*y; }


// https://www.shadertoy.com/view/4djSRW
float hash12(vec2 p)
{
	vec3 p3  = fract(vec3(p.xyx) * .1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
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


// const int START_N = startN;
// const int MAX_N = maxN;
// #define BLUE_JUMP jumpFunc(MAX_N)  // MAX_N

void handleNeighbour(ivec2 otherCoord, vec2 fcoord, bool onBlue, inout bool[2] lastSettled, inout float[2] minForward)
{
    vec4 otherVal = texelFetch(bufferA, otherCoord, 0);

    bool otherOnBlue = otherCoord % BLUE_JUMP == ivec2(0);

    // bool zeroDet = false;

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
            // bool zeroDet = false;
            for(int i = 0; i < (otherYellowBlue == 1 ? 5 : 2) && maxVal(abs(fcoord - surfProj)) > 0.5; i++) 
            {
                sj = transpose(surfaceAutoDiff(mat3x2(ts, 1, 0, 0, 1)));

                surf = sj[0];
                surfProj = proj(surf);

                mat2x2 pj = PROJJACOB(sj);
                // if(abs(determinant(pj)) < 10. * 1000.)
                // {
                //     zeroDet = true;
                //     // break;
                // }

                ts += (fcoord - surfProj) * inverse(pj);
            }

            if(any(isnan(ts)) || !inRange(ts)) continue;

            surf = surface(ts);
            surfProj = proj(surf);


            bool newSettled = maxVal(abs(fcoord - surfProj)) <= 0.5;

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

                        lastSettled[yellowBlue] = true;
                        minForward[yellowBlue] = forwardDir;
                    }
                }
            }
        }
    }

    // return zeroDet;
}

void main( void )
{    
    ivec2 coord = ivec2(gl_FragCoord);
    vec2 fcoord = floor(gl_FragCoord.xy);


    if(frame == 0)
    {
        const vec2 initTS = vec2(0.5, 0.5);

        if(sqi(coord - ivec2(proj(surface(initTS)))) < 4)
            fragColor = vec4(initTS, none, none);
        else
            fragColor = vec4(none);

        return;
    }

    bool onBlue = coord % BLUE_JUMP == ivec2(0);

    bool[] lastSettled = bool[2](false, false);
    float[] minForward = float[2](-1., -1.);


    fragColor = vec4(none);

    // Run once for otherCoord = coord
    handleNeighbour(coord, fcoord, onBlue, lastSettled, minForward);

    // if(zeroDet) 
    // {
    //     fragColor = vec4(none);
    //     return;
    // }

    // int N = 1 + (int(343. * hash12(vec2(2 * frame + START_N, coord.x + resolution.x * coord.y) / 421.)) % 3);
    for(int n = START_N; n <= MAX_N; n++)
    {
        int jf = jumpFunc(n);

        // if(false && jf == 1)
        // {
        //     for(int i = 1; i < 8; i += 2 /* variable (1, 2) */)
        //     {
        //         ivec2 otherCoord = coord + jf * relSquare(i);


        //         if(any(lessThan(otherCoord, ivec2(0))) || any(greaterThanEqual(otherCoord, resolution))) continue;


        //         handleNeighbour(otherCoord, fcoord, onBlue, lastSettled, minForward);
        //     }
        // }
        // else
        {
            int i = 1 + 2 * (int(100. * hash12(vec2(2 * frame + START_N, coord.x + resolution.x * coord.y) / 100.)) % 4);
            // int i = 1 + 2 * (((3 * frame + START_N) + coord.x + resolution.x * coord.y) % 4);
            {
                ivec2 otherCoord = coord + jf * relSquare(i);


                if(any(lessThan(otherCoord, ivec2(0))) || any(greaterThanEqual(otherCoord, resolution))) continue;


                handleNeighbour(otherCoord, fcoord, onBlue, lastSettled, minForward);
            }
        }
    }
}



