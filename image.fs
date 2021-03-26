precision highp float;

uniform sampler2D position3D;


out vec4 fragColor;

void main( void )
{
    ivec2 coord = ivec2(gl_FragCoord);

    vec4 val = texelFetch(bufferA, coord, 0);

    if(MODE(val) == NONE)
    {
        fragColor = vec4(1, 0, 1, 1);
        return;
    }

    vec3 surfVal = texelFetch(position3D, coord, 0).xyz;

    // if(sq(proj(surfVal) - gl_FragCoord.xy) > sq(1.))
    // {
    //     fragColor = vec4(1, 1, 0, 1);
    //     return;
    // }

    //fragColor = vec4(vec3(sqrt(sq(proj(surface(val.xy)) - gl_FragCoord.xy))), 1);

    //fragColor = vec4(val.xyz, 1);



    // if(sq(proj(surface(vec2(0., 0.))) - gl_FragCoord.xy) <= sq(10.))
    // {
    //     fragColor = vec4(0, 0, 0, 1);
    // }
    // if(sq(proj(surface(vec2(1., 0.))) - gl_FragCoord.xy) <= sq(10.))
    // {
    //     fragColor = vec4(1, 0, 0, 1);
    // }
    // if(sq(proj(surface(vec2(0., 1.))) - gl_FragCoord.xy) <= sq(10.))
    // {
    //     fragColor = vec4(0, 1, 0, 1);
    // }
    // if(sq(proj(surface(vec2(1., 1.))) - gl_FragCoord.xy) <= sq(10.))
    // {
    //     fragColor = vec4(1, 1, 0, 1);
    // }


    mat2x3 sj = surfaceJacob(val.xy, surface(val.xy));
    vec3 normal = normalize(cross(sj[0], sj[1]));

    float light = dot(normal, vec3(1, 0, 0)) / 2. + 0.5;

    fragColor = vec4(vec3(light), 1);


}