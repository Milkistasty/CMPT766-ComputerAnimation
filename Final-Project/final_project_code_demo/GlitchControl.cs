// Student ID: 301586596
// Student Name: Wenhe Wang
// Student Email: wwa118@sfu.ca

using System.Collections;
using UnityEngine;

// Periodically changing the _GlitchIntensity value to achieve random "transmission instabilities" effect

public class GlitchControl : MonoBehaviour
{
    public float glitchChance = 0.1f;  // init glitchChance

    Material hologramMaterial;  // Material for the hologram
    WaitForSeconds glitchLoopWait = new WaitForSeconds(0.2f);  // Time between glitch checks

    void Awake()
    {
        hologramMaterial = GetComponent<Renderer>().material;  // Access the material of the object
        glitchChance = hologramMaterial.GetFloat("_GlitchChance");  // update our glitch chance from our hologram.shader
    }

    IEnumerator Start()
    {
        while (true)
        {
            float glitchTest = Random.Range(0f, 1f);  // Randomly decide if a glitch will occur

            if (glitchTest <= glitchChance)  // If a glitch occurs
            {
                float originalGlowIntensity = hologramMaterial.GetFloat("_GlowIntensity");  // Get current glow intensity

                // Set a random GlitchIntensity for the glitch effect
                hologramMaterial.SetFloat("_GlitchIntensity", Random.Range(0.07f, 0.1f));

                // Randomize GlowIntensity within a range to simulate instability
                hologramMaterial.SetFloat("_GlowIntensity", originalGlowIntensity * Random.Range(0.14f, 0.44f));

                // Wait for a random time before resetting the glitch
                yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));

                // Reset GlitchIntensity and GlowIntensity after the glitch period
                hologramMaterial.SetFloat("_GlitchIntensity", 0f);
                hologramMaterial.SetFloat("_GlowIntensity", originalGlowIntensity);
            }

            yield return glitchLoopWait;
        }
    }
}