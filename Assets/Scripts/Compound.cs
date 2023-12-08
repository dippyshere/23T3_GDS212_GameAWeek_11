using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] // This attribute allows the class to be serialized and displayed in the inspector
public class Compound
{
    [SerializeField, Tooltip("The name of the compound")]
    public CompoundType name;

    /// <summary>
    /// Checks if the compound is assembled correctly
    /// </summary>
    /// <returns>True if the compound is assembled correctly, false otherwise</returns>
    public bool IsAssembled()
    {
        // find all the connectors in the scene
        GameObject[] connectors = GameObject.FindGameObjectsWithTag("Connector");
        // check if the compound is assembled correctly
        switch (name)
        {
            // For the water compound...
            case CompoundType.Water:
                // Create a list of booleans to store whether or not each connector is attached to the correct atoms correctly
                List<bool> attached = new List<bool>();
                // For each connector in the scene...
                foreach (GameObject connector in connectors)
                {
                    // Get the AtomManager component from the connector
                    AtomManager atomManager = connector.GetComponent<AtomManager>();
                    // Check if the connector is attached to a hydrogen atom and an oxygen atom
                    if (atomManager.connectedAtomTypes.Contains(AtomType.Hydrogen) && atomManager.connectedAtomTypes.Contains(AtomType.Oxygen))
                    {
                        // If the connector is attached to a hydrogen atom and an oxygen atom, add true to the list of attached connectors
                        attached.Add(true);
                    }
                    else
                    {
                        // If the connector is not attached to a hydrogen atom and an oxygen atom, add false to the list of attached connectors
                        attached.Add(false);
                    }
                }
                // Check if any of the connectors are not attached to the correct atoms or if there are no connectors in the scene
                if (attached.Contains(false) || attached.Count == 0)
                {
                    // If any of the connectors are not attached to the correct atoms or if there are no connectors in the scene, return false
                    return false;
                }
                else
                {
                    // If all of the connectors are attached to the correct atoms, return true
                    return true;
                }
            case CompoundType.Methane:
                List<bool> attached2 = new List<bool>();
                foreach (GameObject connector in connectors)
                {
                    AtomManager atomManager = connector.GetComponent<AtomManager>();
                    if (atomManager.connectedAtomTypes.Contains(AtomType.Hydrogen) && atomManager.connectedAtomTypes.Contains(AtomType.Carbon))
                    {
                        attached2.Add(true);
                    }
                    else
                    {
                        attached2.Add(false);
                    }
                }
                if (attached2.Contains(false) || attached2.Count == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            case CompoundType.Ammonia:
                List<bool> attached3 = new List<bool>();
                foreach (GameObject connector in connectors)
                {
                    AtomManager atomManager = connector.GetComponent<AtomManager>();
                    if (atomManager.connectedAtomTypes.Contains(AtomType.Hydrogen) && atomManager.connectedAtomTypes.Contains(AtomType.Nitrogen))
                    {
                        attached3.Add(true);
                    }
                    else
                    {
                        attached3.Add(false);
                    }
                }
                if (attached3.Contains(false) || attached3.Count == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            case CompoundType.Methanol:
                List<bool> attached4 = new List<bool>();
                foreach (GameObject connector in connectors)
                {
                    AtomManager atomManager = connector.GetComponent<AtomManager>();
                    if (atomManager.connectedAtomTypes.Contains(AtomType.Hydrogen) && atomManager.connectedAtomTypes.Contains(AtomType.Carbon))
                    {
                        attached4.Add(true);
                    }
                    else if (atomManager.connectedAtomTypes.Contains(AtomType.Carbon) && atomManager.connectedAtomTypes.Contains(AtomType.Oxygen))
                    {
                        attached4.Add(true);
                    }
                    else if (atomManager.connectedAtomTypes.Contains(AtomType.Hydrogen) && atomManager.connectedAtomTypes.Contains(AtomType.Oxygen))
                    {
                        attached4.Add(true);
                    }
                    else
                    {
                        attached4.Add(false);
                    }
                }
                if (attached4.Contains(false) || attached4.Count == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            case CompoundType.AmmoniumHydroxide:
                List<bool> attached5 = new List<bool>();
                foreach (GameObject connector in connectors)
                {
                    AtomManager atomManager = connector.GetComponent<AtomManager>();
                    if (atomManager.connectedAtomTypes.Contains(AtomType.Hydrogen) && atomManager.connectedAtomTypes.Contains(AtomType.Nitrogen))
                    {
                        attached5.Add(true);
                    }
                    else if (atomManager.connectedAtomTypes.Contains(AtomType.Hydrogen) && atomManager.connectedAtomTypes.Contains(AtomType.Oxygen))
                    {
                        attached5.Add(true);
                    }
                    else
                    {
                        attached5.Add(false);
                    }
                }
                if (attached5.Contains(false) || attached5.Count == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            case CompoundType.AcetateAcid:
                List<bool> attached6 = new List<bool>();
                foreach (GameObject connector in connectors)
                {
                    AtomManager atomManager = connector.GetComponent<AtomManager>();
                    if (atomManager.connectedAtomTypes.Contains(AtomType.Hydrogen) && atomManager.connectedAtomTypes.Contains(AtomType.Carbon))
                    {
                        attached6.Add(true);
                    }
                    else if (atomManager.connectedAtomTypes.Contains(AtomType.Carbon) && atomManager.connectedAtomTypes.Contains(AtomType.Oxygen))
                    {
                        attached6.Add(true);
                    }
                    else if (atomManager.connectedAtomTypes.Contains(AtomType.Hydrogen) && atomManager.connectedAtomTypes.Contains(AtomType.Oxygen))
                    {
                        attached6.Add(true);
                    }
                    else if (atomManager.connectedAtomTypes.Contains(AtomType.Carbon) && atomManager.connectedAtomTypes.Contains(AtomType.Carbon))
                    {
                        attached6.Add(true);
                    }
                    else
                    {
                        attached6.Add(false);
                    }
                }
                if (attached6.Contains(false) || attached6.Count == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            // Add more cases for additional compounds here below here

            // If the compound is not one of the above compounds, return false
            default:
                return false;
        }
    }

    /// <summary>
    /// The types of atoms that can be used to assemble compounds
    /// </summary>
    public enum AtomType
    {
        Hydrogen,
        Carbon,
        Nitrogen,
        Oxygen,
        _Connector
    }

    /// <summary>
    /// The types of compounds that can be assembled
    /// </summary>
    public enum CompoundType
    {
        None,
        Water,
        Methane,
        Ammonia,
        Methanol,
        AmmoniumHydroxide,
        AcetateAcid
    }

}
