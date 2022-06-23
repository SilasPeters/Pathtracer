#version 330
/*
// source: 2014 lecture slides
 
// shader input
in vec2 vUV;			// vertex uv coordinate
in vec3 vNormal;		// untransformed vertex normal
in vec3 vPosition;		// untransformed vertex position

// shader output
out vec4 normal;		// transformed vertex normal
out vec2 uv;				
uniform mat4 transform;
 
// vertex shader
void main()
{
	// transform vertex using supplied matrix
	gl_Position = transform * vec4(vPosition, 1.0);

	// forward normal and uv coordinate; will be interpolated over triangle
	normal = transform * vec4( vNormal, 0.0f );
	uv = vUV;
}
*/
// inputs
in vec3 vPosition; // object space
in vec3 vNormal; // object space
in vec2 vUV;
uniform mat4 objectToScreen;
uniform mat4 objectToWorld;
// outputs
out vec4 position; // world space
out vec4 normal; // world space
out vec2 uv;
// vertex shader
void main()
{
	gl_Position = objectToScreen * vec4(vPosition, 1.0);
	position = objectToWorld * vec4(vPosition, 1.0);
	normal = objectToWorld * vec4(vNormal, 0.0); // works only if objectToWorld is a similarity transformation!
	uv = vUV; // pass-through // (no shearing or non-uniform scaling component)
}
