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
#define sq(v) dot(v, v)

float maxVal(vec2 v) { return max(v.x, v.y); }


////////////////////
///   AutoDiff   ///
////////////////////

/// Types ///

#define VAL vec3
#define VAL2 mat2x3
#define VAL3 mat3x3


/// Simple Functions/Operators ///

#define c(a) VAL(a, 0, 0)

VAL add(VAL a, VAL b) { return a + b; }
VAL sub(VAL a, VAL b) { return a - b; }

#if 0
VAL mul(VAL a, VAL b) { return VAL(a.x*b.x, a.y*b.x + b.y*a.x, a.z*b.x + b.z*a.x); }
#else
VAL mul(VAL a, VAL b) { return VAL(a.x*b.x, dot(a.yx, b.xy), dot(a.zx, b.xz)); }
#endif

VAL recip(VAL b) { return VAL(1. / b.x, -b.yz / sq(b.x)); }  // reciprocal

#if 0
VAL div(VAL a, VAL b) { return mul(a, recip(b)); }
#else
const vec2 negateY = vec2(1, -1);
VAL div(VAL a, VAL b) { return VAL(a.x/b.x, vec2(dot(a.yx, b.xy*negateY), dot(a.zx, b.xz*negateY)) / sq(b.x)); }
#endif


/// Other Functions ///

VAL d_sq(VAL a) { return VAL(sq(a.x), 2.*a.x*a.yz); }
VAL d_sin(VAL a) { return VAL(sin(a.x), cos(a.x) * a.yz); }
VAL d_cos(VAL a) { return VAL(cos(a.x), -sin(a.x) * a.yz); }
VAL d_exp(VAL a) { return exp(a.x) * VAL(1, a.yz); }
VAL d_log(VAL a) { return VAL(log(a.x), a.yz / a.x); }


// TODO: maybe I can do the same thing I did with cross(VAL3, VAL3) for 'mul' and 'div'

VAL2 mul(VAL  a, VAL2 b) { return VAL2(mul(a   , b[0]), mul(a   , b[1])); }
VAL2 mul(VAL2 a, VAL  b) { return VAL2(mul(a[0], b   ), mul(a[1], b   )); }
VAL2 mul(VAL2 a, VAL2 b) { return VAL2(mul(a[0], b[0]), mul(a[1], b[1])); }

VAL3 mul(VAL  a, VAL3 b) { return VAL3(mul(a   , b[0]), mul(a   , b[1]), mul(a   , b[2])); }
VAL3 mul(VAL3 a, VAL  b) { return VAL3(mul(a[0], b   ), mul(a[1], b   ), mul(a[2], b   )); }
VAL3 mul(VAL3 a, VAL3 b) { return VAL3(mul(a[0], b[0]), mul(a[1], b[1]), mul(a[2], b[2])); }


VAL2 div(VAL  a, VAL2 b) { return VAL2(div(a   , b[0]), div(a   , b[1])); }
VAL2 div(VAL2 a, VAL  b) { return VAL2(div(a[0], b   ), div(a[1], b   )); }
VAL2 div(VAL2 a, VAL2 b) { return VAL2(div(a[0], b[0]), div(a[1], b[1])); }

VAL3 div(VAL  a, VAL3 b) { return VAL3(div(a   , b[0]), div(a   , b[1]), div(a   , b[2])); }
VAL3 div(VAL3 a, VAL  b) { return VAL3(div(a[0], b   ), div(a[1], b   ), div(a[2], b   )); }
VAL3 div(VAL3 a, VAL3 b) { return VAL3(div(a[0], b[0]), div(a[1], b[1]), div(a[2], b[2])); }

VAL d_lenSq(VAL3 v) { return d_sq(v[0]) + d_sq(v[1]) + d_sq(v[2]); }

#if 1
VAL d_sqrt(VAL a) { float sqrtX = sqrt(a.x); return VAL(sqrtX, 0.5 * a.yz / sqrtX); }
#else
VAL d_sqrt(VAL a) { return sqrt(a.x) * VAL(1., 0.5 * a.yz / a.x); }
#endif


VAL3 d_cross(VAL3 a, VAL3 b) 
{
    mat3 at = transpose(a);
    mat3 bt = transpose(b);
    
    return transpose(mat3(cross(at[0], bt[0]), cross(at[1], bt[0]) + cross(at[0], bt[1]), cross(at[2], bt[0]) + cross(at[0], bt[2])));
}



/// Complex Functions ///

VAL2 dc_mul(VAL2 a, VAL2 b) { return VAL2(mul(a[0], b[0]) - mul(a[1], b[1]), mul(a[1], b[0]) + mul(a[0], b[1])); }
VAL2 dc_sq(VAL2 z) { return VAL2(d_sq(z[0]) - d_sq(z[1]), 2. * mul(z[0], z[1])); }

#if 0
VAL2 dc_conj(VAL2 z) { return VAL2(z[0], -z[1]); } // conjugate
#else
const VAL2 c_negateY = mat2x3(vec3(1), vec3(-1));
VAL2 dc_conj(VAL2 z) { return matrixCompMult(z, c_negateY); } // conjugate
#endif

VAL dc_absSq(VAL2 z) { return d_sq(z[0]) + d_sq(z[1]); }

VAL2 dcr_div(VAL2 z, VAL b) { return VAL2(div(z[0], b), div(z[1], b)); }
VAL2 dc_recip(VAL2 z) { return dcr_div(dc_conj(z), dc_absSq(z)); }


///////////////////////////////////
///   the Parametric Equation   ///
///////////////////////////////////

{{parametricEquation}}










// // Complex functions
// vec2 cMul(vec2 a, vec2 b) { return vec2(a.x*b.x - a.y*b.y, a.x*b.y + a.y*b.x); }
// vec2 cSq(vec2 z) { return vec2(sq(z.x) - sq(z.y), 2.*z.x*z.y); }
// vec2 cCon(vec2 z) { return vec2(z.x, -z.y); }
// float cSqAbs(vec2 z) { return sq(z.x) + sq(z.y); }
// vec2 cRecip(vec2 b) { return cCon(b) / cSqAbs(b); }  // 1/b = cCon(b) / ( b*cCon(b) ) = cCon(b) / cSqAbs(b)


// vec3 surface(vec2 s)
// {
//     const float sqrt5 = sqrt(5.);
    
//     vec2 s2 = cSq(s);
//     vec2 s3 = cMul(s2, s);
//     vec2 s4 = cSq(s2);
//     vec2 s6 = cSq(s3);
    
//     const vec2 one = vec2(1, 0);
    
//     vec2 denominator = cRecip(s6 + sqrt5*s3 - one);
    
//     vec3 g = vec3(
//         -1.5 * cMul(cMul(s, one - s4), denominator).y, 
//         -1.5 * cMul(cMul(s, one + s4), denominator).x, 
//         cMul(one + s6, denominator).y - 0.5
//     );
        
//     return g / (sq(g.x) + sq(g.y) + sq(g.z));
// }

// x + y * i
// mat2x3(x, dx/dt, dx/ds, y, dy/dt, dy/ds)
// mat3x2(x, y, dx/dt, dy/dt, dx/ds, dy/ds)
// mat3x2(ts.x, ts.y, 1, 0, 0, 1)


// mat3x2 d_cMul(mat3x2 a, mat3x2 b) { return mat3x2(cMul(a[0], b[0]), cMul(a[1], b[0]) + cMul(a[0], b[1]), cMul(a[2], b[0]) + cMul(a[0], b[2])); }
// mat3x2 d_cSq(mat3x2 z) { return mat3x2(cSq(z[0]), 2.*cMul(z[0], z[1]), 2.*cMul(z[0], z[2])); }
// mat3x2 d_cCon(mat3x2 z) { return matrixCompMult(z, mat3x2(1, -1, 1, -1, 1, -1)); }
// vec3 d_cSqAbs(mat3x2 z) { return vec3(sq(z[0].x) + sq(z[0].y), 2. * z[0] * mat2(z[1], z[2])); }

// vec3 d_recip(vec3 b) { return vec3(1. / b.x, -b.yz / sq(b.x)); }

// vec3 d_mul(vec3 a, vec3 b) { return vec3(a.x*b.x, a.yz*b.x + b.yz*a.x); }
// mat3x2 d_rMul(mat3x2 a, vec3 b) { mat2x3 a_t = transpose(a); return transpose(mat2x3(d_mul(a_t[0], b), d_mul(a_t[1], b))); }

// mat3x2 d_cRecip(mat3x2 b) { return d_rMul(d_cCon(b), d_recip(d_cSqAbs(b))); }  // 1/b = cCon(b) / ( b*cCon(b) ) = cCon(b) / cSqAbs(b)

// vec3 d_sq(vec3 a) { return vec3(sq(a.x), 2.*a.x*a.yz); }



// mat3x3 surface(mat3x2 s)
// {
//     const float sqrt5 = sqrt(5.);
    
//     mat3x2 s2 = d_cSq(s);
//     mat3x2 s3 = d_cMul(s2, s);
//     mat3x2 s4 = d_cSq(s2);
//     mat3x2 s6 = d_cSq(s3);
    
//     const mat3x2 one = mat3x2(1, 0, 0, 0, 0, 0);
    
//     mat3x2 denominator = d_cRecip(s6 + sqrt5*s3 - one);
    
//     mat3x3 g = VAL3(
//         -1.5 * transpose(d_cMul(d_cMul(s, one - s4), denominator))[1],
//         -1.5 * transpose(d_cMul(d_cMul(s, one + s4), denominator))[0],
//         transpose(d_cMul(one + s6, denominator))[1] - vec3(0.5, 0, 0)
//     );

//     vec3 normalFactor = d_recip(d_sq(g[0]) + d_sq(g[1]) + d_sq(g[2]));

//     return mat3x3(d_mul(g[0], normalFactor), d_mul(g[1], normalFactor), d_mul(g[2], normalFactor));
// }



// const float dt = 0.01;


// mat2x3 surfaceJacob(vec2 ts, vec3 pos)
// {
//     // return mat2x3((surface(ts + vec2(dt, 0)) - pos) / dt,    // d(surface(t, s))/dt
//                 //   (surface(ts + vec2(0, dt)) - pos) / dt);   // d(surface(t, s))/ds

//     // return mat2x3(transpose(surface(vec4(ts.x, 1, ts.y, 0)))[1],
//     //               transpose(surface(vec4(ts.x, 0, ts.y, 1)))[1]);

//     mat3x3 autodiffRes_t = transpose(surface(VAL2(VAL(ts.x, 1, 0), VAL(ts.y, 0, 1))));

//     return mat2x3(autodiffRes_t[1], autodiffRes_t[2]);

// }


#if 0
vec2 proj(vec3 v)  // TODO: define?
{
    return vec2(vec4(v, 1) * projMat);
    // return v * mat2x3(projMat) + projMat[3]
}
#else
#define proj(v) vec2(vec4(v, 1) * projMat)
#endif

#define ISNONE(x) (abs(x - none) < 0.01)
