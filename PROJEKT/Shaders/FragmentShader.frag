#version 330 core
out vec4 FragColor;

uniform vec3 uLightColor;
uniform vec3 uViewPos;

uniform float uShininess;

uniform sampler2D uTexture;
		
in vec4 outCol;
in vec3 outNormal;
in vec3 outWorldPosition;
in vec2 outTexture;

void main()
{
    vec3 lightDir = normalize(vec3(-1.0, -1.0, -0.3));

    float ambientStrength = 0.2;
    vec3 ambient = ambientStrength * uLightColor;

    float diffuseStrength = 0.01;
    vec3 norm = normalize(outNormal);
    float diff = max(dot(norm, -lightDir), 0.0);
    vec3 diffuse = diff * uLightColor * diffuseStrength;

    float specularStrength = 0.01;
    vec3 viewDir = normalize(uViewPos - outWorldPosition);
    vec3 reflectDir = reflect(lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), uShininess);
    vec3 specular = specularStrength * spec * uLightColor;

    vec4 textureColor = texture(uTexture, outTexture);

    vec3 result = (ambient + diffuse + specular) * outCol.rgb + textureColor.rgb;
    FragColor = vec4(result, outCol.a);
}