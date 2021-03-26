precision highp float;

out vec4 fragColor;

void main( void )
{    
    ivec2 coord = ivec2(gl_FragCoord);

    vec4 val = texelFetch(bufferA, ivec2(gl_FragCoord), 0);

    if(MODE(val) == ACTIVE)
    {
        fragColor = vec4(surface(texelFetch(bufferA, coord, 0).xy), ACTIVE);

        return;
    }
    else
    {
        fragColor = vec4(vec3(0), NONE);
    }
}

    