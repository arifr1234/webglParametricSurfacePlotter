precision highp float;


out vec4 fragColor;

void main( void )
{
    ivec2 coord = ivec2(gl_FragCoord);

    vec4 val = texelFetch(bufferA, coord, 0);

    // if(!ISNONE(val.z))
    // {
    //     fragColor = vec4(1, 0, 0, 1);
    //     return;
    // }

    if(ISNONE(val.x))//(MODE(val) == NONE)
    {
        fragColor = vec4(1, 0, 1, 1);
        return;
    }

    mat3x3 surfJacob = transpose(surface(VAL2(VAL(val.x, 1, 0), VAL(val.y, 0, 1))));

    vec3 surfVal = surfJacob[0];  // surface(val.xy);

    // if(maxVal(abs(gl_FragCoord.xy - proj(surfVal))) > 1.)
    // {
    //     fragColor = vec4(1, 1, 0, 1);
    //     return;
    // }


    vec3 normal = normalize(cross(surfJacob[1], surfJacob[2]));

    float light = dot(normal, vec3(1, 0, 0)) / 2. + 0.5;

    fragColor = vec4(vec3(light), 1);


}