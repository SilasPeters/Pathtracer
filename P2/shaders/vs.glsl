#version 330
	
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

/* normal code
// inputs
in vec3 vertexPosition; // object space
in vec3 vertexNormal; // object space
in vec2 vertexUV;
uniform mat4 objectToScreen;
// outputs
out vec4 position; // world space
out vec4 normal; // world space
out vec2 uv;
// vertex shader
void main()
{
	gl_Position = objectToScreen * vec4(vertexPosition, 1.0);
	position = vec4(vertexPosition, 1.0);
	normal = vec4(vertexNormal, 0.0);
	uv = vertexUV; // pass-through // (no shearing or non-uniform scaling component)
}*/
/*	
// testing code
layout (location = 0) in vec3 vertexPosition; // the position variable has attribute position 0
  
out vec4 vertexColor; // specify a color output to the fragment shader

void main()
{
    gl_Position = vec4(vertexPosition, 1.0); // see how we directly give a vec3 to vec4's constructor
    vertexColor = vec4(0.5, 0.0, 0.0, 1.0); // set the output variable to a dark-red color
}
*/