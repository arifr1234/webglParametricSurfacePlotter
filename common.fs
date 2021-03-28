precision highp float;

#define TAU 6.28318530718

uniform float time;
uniform int frame;
uniform highp ivec2 resolution;

uniform sampler2D bufferA;

uniform float none;


uniform mat3x3 axes;
uniform mat2x4 projMat;



int sqi(ivec2 v) { return v.x*v.x + v.y*v.y; }
int sqi(int v) { return v*v; }
#define sqFunc(type) float sq(type v) { return dot(v, v); }
sqFunc(vec2)
sqFunc(float)

float maxVal(vec2 v) { return max(v.x, v.y); }



// Complex functions
vec2 cMul(vec2 a, vec2 b) { return vec2(a.x*b.x - a.y*b.y, a.x*b.y + a.y*b.x); }
vec2 cSq(vec2 z) { return vec2(sq(z.x) - sq(z.y), 2.*z.x*z.y); }
vec2 cCon(vec2 z) { return vec2(z.x, -z.y); }
float cSqAbs(vec2 z) { return sq(z.x) + sq(z.y); }
vec2 cRecip(vec2 b) { return cCon(b) / cSqAbs(b); }  // 1/b = cCon(b) / ( b*cCon(b) ) = cCon(b) / cSqAbs(b)


vec3 surface(vec2 s)
{
    const float sqrt5 = sqrt(5.);
    
    vec2 s2 = cSq(s);
    vec2 s3 = cMul(s2, s);
    vec2 s4 = cSq(s2);
    vec2 s6 = cSq(s3);
    
    const vec2 one = vec2(1, 0);
    
    vec2 denominator = cRecip(s6 + sqrt5*s3 - one);
    
    vec3 g = vec3(
        -1.5 * cMul(cMul(s, one - s4), denominator).y, 
        -1.5 * cMul(cMul(s, one + s4), denominator).x, 
        cMul(one + s6, denominator).y - 0.5
    );
        
    return g / (sq(g.x) + sq(g.y) + sq(g.z));
}

bool inRange(vec2 ts)
{
    return cSqAbs(ts) <= 1.;
}


// x + y * i
// mat2x3(x, dx/dt, dx/ds, y, dy/dt, dy/ds)
// mat3x2(x, y, dx/dt, dy/dt, dx/ds, dy/ds)
// mat3x2(ts.x, ts.y, 1, 0, 0, 1)


mat3x2 d_cMul(mat3x2 a, mat3x2 b) { return mat3x2(cMul(a[0], b[0]), cMul(a[1], b[0]) + cMul(a[0], b[1]), cMul(a[2], b[0]) + cMul(a[0], b[2])); }
mat3x2 d_cSq(mat3x2 z) { return mat3x2(cSq(z[0]), 2.*cMul(z[0], z[1]), 2.*cMul(z[0], z[2])); }
mat3x2 d_cCon(mat3x2 z) { return matrixCompMult(z, mat3x2(1, -1, 1, -1, 1, -1)); }
vec3 d_cSqAbs(mat3x2 z) { return vec3(sq(z[0].x) + sq(z[0].y), 2. * z[0] * mat2(z[1], z[2])); }

vec3 d_recip(vec3 b) { return vec3(1. / b.x, -b.yz / sq(b.x)); }

vec3 d_mul(vec3 a, vec3 b) { return vec3(a.x*b.x, a.yz*b.x + b.yz*a.x); }
mat3x2 d_rMul(mat3x2 a, vec3 b) { mat2x3 a_t = transpose(a); return transpose(mat2x3(d_mul(a_t[0], b), d_mul(a_t[1], b))); }

mat3x2 d_cRecip(mat3x2 b) { return d_rMul(d_cCon(b), d_recip(d_cSqAbs(b))); }  // 1/b = cCon(b) / ( b*cCon(b) ) = cCon(b) / cSqAbs(b)

vec3 d_sq(vec3 a) { return vec3(sq(a.x), 2.*a.x*a.yz); }



mat3x3 surfaceAutoDiff(mat3x2 s)
{
    const float sqrt5 = sqrt(5.);
    
    mat3x2 s2 = d_cSq(s);
    mat3x2 s3 = d_cMul(s2, s);
    mat3x2 s4 = d_cSq(s2);
    mat3x2 s6 = d_cSq(s3);
    
    const mat3x2 one = mat3x2(1, 0, 0, 0, 0, 0);
    
    mat3x2 denominator = d_cRecip(s6 + sqrt5*s3 - one);
    
    mat3x3 g = mat3x3(
        -1.5 * transpose(d_cMul(d_cMul(s, one - s4), denominator))[1],
        -1.5 * transpose(d_cMul(d_cMul(s, one + s4), denominator))[0],
        transpose(d_cMul(one + s6, denominator))[1] - vec3(0.5, 0, 0)
    );

    vec3 normalFactor = d_recip(d_sq(g[0]) + d_sq(g[1]) + d_sq(g[2]));

    return mat3x3(d_mul(g[0], normalFactor), d_mul(g[1], normalFactor), d_mul(g[2], normalFactor));
}



// const float dt = 0.01;


mat2x3 surfaceJacob(vec2 ts, vec3 pos)
{
    // return mat2x3((surface(ts + vec2(dt, 0)) - pos) / dt,    // d(surface(t, s))/dt
                //   (surface(ts + vec2(0, dt)) - pos) / dt);   // d(surface(t, s))/ds

    // return mat2x3(transpose(surfaceAutoDiff(vec4(ts.x, 1, ts.y, 0)))[1],
    //               transpose(surfaceAutoDiff(vec4(ts.x, 0, ts.y, 1)))[1]);

    mat3x3 autodiffRes_t = transpose(surfaceAutoDiff(mat3x2(ts, 1, 0, 0, 1)));

    return mat2x3(autodiffRes_t[1], autodiffRes_t[2]);

}


vec2 proj(vec3 v)  // TODO: define?
{
    return vec2(vec4(v, 1) * projMat);
    // return v * mat2x3(projMat) + projMat[3]
}

#define ISNONE(x) (abs(x - none) < 0.01)
