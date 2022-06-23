#version 330

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
/* 
// inputs
in vec4 position; // world space
in vec4 normal; // world space
in vec2 uv;
uniform vec3 cameraPosition; // world space
uniform vec3 lightPosition; // world space
uniform vec3 lightColor; // RGB
uniform sampler2D tex; // diffuse color
// output
out vec4 color; // RGBA
// fragment shader
void main()
{
    color = ... // insert shading calculations here
}
*/    