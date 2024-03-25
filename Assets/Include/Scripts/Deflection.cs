using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using System;

public class Deflection : MonoBehaviour{
    // Variables for computing deflection
    private static int n; // Number of points for computing the beam
    private double[] delta_b; // Deflection across the beam

    private float L = 3.0f; // Length (m)
    private float h = 0.2f; // Height (m)
    private float b = 0.05f; // Base (m)

    private float I; // Inertia (m^4)
    private float c; 
    private float E; // Young's Modulus (Pa)

    private double P = 1000; // Load Magnitude (N)
    private float Px; // Load position (m)

    // Dictionary with mechanical properties from multiple variables
    // [Stress allowed (MPa), Young's Modulus (GPa)]
    private Dictionary<string, int[]> materialProperties = new Dictionary<string, int[]>()
    {
        {"Aluminum", new int[] {110, 69}}, // Aluminum 1050-H14
        {"Copper",  new int[] {344, 110}}, // Copper, Cu; Cold Drawn
        {"Steel",  new int[] {440, 200}}, // AISI 1018 Steel, cold drawn
        {"Wood", new int[] {4, 12}}, // Bigleaf Maple Wood
        {"Zinc", new int[] {37, 96}}, // Pure Zinc
        {"Marble", new int[] {20, 60}}, // Pure Marble
        {"Rusted", new int[] {275, 125}}, // Reduced 37.5% from AISI 1018 Steel, cold drawn
        {"Cement", new int[] {1, 11}} // Portland Cement
    };

    // Properties from the scene
    [SerializeField] private Transform load; // Load transform object
    [SerializeField] private TMP_Text loadMagnitude; // Display for load magnitude
    [SerializeField] private TMP_Text lengthMagnitude; // Display for length magnitude
    [SerializeField] private TMP_Text heightMagnitude; // Display for height magnitude
    [SerializeField] private TMP_Text baseMagnitude; // Display for base magnitude
    [SerializeField] private TMP_Text display; // Display for keypad

    // Variables for code functionality
    [SerializeField] private float magnification_constant = 1e-1f; // Magnification constant for the deflection
    private GameObject beam; // Beam object (agroupates all the points from the beam)
    private Component[] children; // All children from the beam (beam points)
    private float initialPos; // Initial "y" position of the beam (no deflection)
    private float initLoad; // Initial "y" position of the load
    private int loadIndex; // Index of the load according to the beam points (approximation)
    private string currVar; // Current variable to change (Load, Height, Length, Base)
    private string currValue; // Value of the current variable to change
    private float PxMin; // Minimum "x" position of the beam
    private float PXMax; // Maximum "x" position of the beam

    void Start(){
        // Initialize variables for computing deflection
        I = b * Mathf.Pow(h, 3) / 12;
        c = h / 2;

        // Get the beam object and the number of points
        beam = transform.GetChild(0).gameObject;
        n = beam.transform.childCount;
        delta_b = new double[n];

        // Get the initial "y" position for the beam and the load
        initialPos = beam.transform.GetChild(0).transform.localPosition.y;
        initLoad = load.localPosition.y;

        PxMin = beam.transform.GetChild(0).transform.localPosition.x;
        PXMax = beam.transform.GetChild(n - 1).transform.localPosition.x;
    }

    // Function for on-click event (before activating the keypad)
    public void getCurrentVariable(string var){
        currVar = var;
        switch (var)
        {
            case "Load":
                currValue = P.ToString();
                break;
            case "Height":
                currValue = h.ToString();
                break;
            case "Length":
                currValue = L.ToString();
                break;
            case "Base":
                currValue = b.ToString();
                break;
            default:
                print("Invalid current variable");
                break;
        }
    }

    // Function for on-click event (for each button in the keypad)
    public void ManageKeypad(string button){
        KeypadOptions(button);
        // Update the keypad display
        if (currVar == "Load")
            display.text = currValue + " N";
        else
            display.text = currValue + " m";
    }

    // Function for managing the keypad options
    private void KeypadOptions(string button){
        switch (button)
        {
            case "Del":
                // Do not delete the last character if the string has only one character
                if (currValue.Length > 1)
                    currValue = currValue.Remove(currValue.Length - 1);
                else
                    currValue = "0";
                break;
            case "Clear":
                currValue = "0";
                break;
            case "Confirm":
                DisplayAndSave();
                break;
            case ".":
                // Do not add a second dot if the string already contains one
                if (!currValue.Contains("."))
                    currValue += ".";
                break;
            default:
                // Add a number to the string
                currValue += button;
                break;
        }

        // Remove the leading zero if the string is not decimal number and has more than one character
        if (currValue[0] == '0' && currValue.Length > 1 && currValue[1] != '.')
            currValue = currValue.Remove(0, 1);
    }

    // Function for displaying and saving the current variable (when quitting the keypad)
    private void DisplayAndSave(){
        switch (currVar)
        {
            case "Load":
                P = double.Parse(currValue);
                loadMagnitude.text = currValue + " N";
                break;
            case "Height":
                h = float.Parse(currValue);
                heightMagnitude.text = currValue + " m";
                break;
            case "Length":
                L = float.Parse(currValue);
                lengthMagnitude.text = currValue + " m";
                break;
            case "Base":
                b = float.Parse(currValue);
                baseMagnitude.text = currValue + " m";
                break;
            default:
                print("Invalid current variable");
                break;
        }
    }

    // Function for resetting the beam to its initial position (no deflection)
    public void Reset(){
        load.localPosition = new Vector3(load.localPosition.x, initLoad, load.localPosition.z); // Reset the load position
        foreach (Transform tr in children)
            tr.localPosition = new Vector3(tr.localPosition.x, initialPos, tr.localPosition.z); // Reset each beam point
    }

    // Function for computing the deflection of the beam
    public void Deflect(){
        Px = (load.localPosition.x - PxMin) / (PXMax - PxMin) * L; // Current load position along the beam (m)

        // Get the current material and its properties
        string currMaterial = beam.transform.GetChild(0).GetComponent<Renderer>().material.name.Split(' ')[1];
        E = materialProperties[currMaterial][1] * 1e9f;

        float x = 0.0f; // Current position along the beam (m)
        float dx = L / n; // Step size for computing the beam
        for (int i = 0; i < n; i++)
        {
            // Deflection equation
            if (x <= Px)
            {
                delta_b[i] = -(P * x * (L - Px) * (Mathf.Pow(L, 2) - Mathf.Pow(L - Px, 2) - Mathf.Pow(x, 2))) / (6 * E * I * L);
                loadIndex = i; // Index of the load according to the beam points (approximation)
            }
            else
                delta_b[i] = -(P * Px * (L - x) * (2 * L * x - Mathf.Pow(x, 2) - Mathf.Pow(Px, 2))) / (6 * E * I * L);
            x += dx;
        }

        Stress(currMaterial);
    }

    // Function for computing the stress and deflecting the beam
    private void Stress(string material)
    {
        // Maximum stress
        double maxSigma = P * Px * (L - Px) / L * c / I;

        // Compare with materials strengths
        if (maxSigma > materialProperties[material][0] * 1e6f)
            print("Cuidado, el material no soporta la carga.");
        else
            print("Echele más ganas mijo.");

        // Get all the transforms from the points in the beam (except the first one, which is the parent object of the beam)
        children = beam.transform.GetComponentsInChildren<Transform>().Skip(1).ToArray();
        
        // Deflect the beam
        int i = 0;
        foreach (Transform tr in children)
        {
            tr.localPosition = new Vector3(tr.localPosition.x, initialPos + (float)delta_b[i]*magnification_constant, tr.localPosition.z);
            i++;
        }

        // Move the load to the top of the beam
        Transform loadAprox = beam.transform.GetChild(loadIndex).transform;
        float y = loadAprox.localPosition.y + loadAprox.localScale.y + 0.019f; // Compute the approximate "y" position for the load
        load.localPosition = new Vector3(load.localPosition.x, y, load.localPosition.z);
    }
}
