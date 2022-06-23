#version 330
/*
//old code

// shader input
in vec2 uv;			// interpolated texture coordinates
in vec4 normal;			// interpolated normal
uniform sampler2D pixels;	// texture sampler

// shader output
out vec4 outputColor;

// fragment shader
void main()
{
    outputColor = texture( pixels, uv ) + 0.5f * vec4( normal.xyz, 1 );
}
*/

// inputs
in vec4 position; // world space
in vec4 normal; // world space
in vec2 uv;
uniform vec3 lightPosition; // world space
uniform vec3 camPos; // world space
uniform vec3 lightColor; // RGB
uniform vec3 ambientLight; // amb light
uniform sampler2D tex; // diffuse color
// output
out vec4 color; // RGBA
// fragment shader // swizzle syntax: select, reorder, or duplicate coordinates using .xyzw or .rgba
void main() // e.g., position.xyz, lightColor.gg, position.x
{
    vec3 L = lightPosition - position.xyz; // vector from surface to light, unnormalized!
    float attenuation = 1.0 / dot(L, L); // distance attenuation
    float NdotL = max(0, dot(normalize(normal.xyz), normalize(L))); // incoming angle attenuation
    vec3 diffuseColor = texture(tex, uv).rgb; // texture lookup
    color = vec4(lightColor * diffuseColor * attenuation * NdotL + ambientLight, 1.0 ); // complete diffuse shading, A = 1.0 is opaque
}
