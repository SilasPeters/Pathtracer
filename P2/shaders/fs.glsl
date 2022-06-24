#version 330

// inputs
in vec4 position; // world space
in vec4 normal; // world space
in vec2 uv;
uniform vec3 lightPosition; // world space
uniform vec3 camPos; // world space
uniform vec3 lightColor; // RGB
uniform vec3 ambientLight; // amb light
uniform sampler2D tex; // diffuse color
uniform vec3 specularCo;

const float glosyness = 50;

// output
out vec4 color; // RGBA


// fragment shader
void main()
{
    vec3 toLight = lightPosition - position.xyz; // vector from surface to light, unnormalized!
    float attenuation = 1.0 / dot(toLight, toLight); // distance attenuation
    float NdotL = max(0, dot(normalize(normal.xyz), normalize(L))); // incoming angle attenuation
    vec3 diffuseColor = texture(tex, uv).rgb; // texture lookup
    //color = vec4(lightColor * diffuseColor * attenuation * NdotL + ambientLight, 1.0 ); // complete diffuse shading, A = 1.0 is opaque

    vec3 V = normalize(camPos - position.xyz); //todo: multiple ls
    vec3 R = normalize(toLight - 2 * dot(normalize(toLight), normal) * normal);
    color = vec4(attenuation * lightColor * diffuseCo * NdotL
                + specularCo * pow(max(0, dot(V, R)), glossyness) + ambientLight
    , 1.0);
}
