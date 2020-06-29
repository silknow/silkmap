﻿using System.Collections;
using System.Collections.Generic;
using Honeti;
using UnityEngine;
using UnityEngine.UI;

public class RelationshipPropertyItem : NormalPropertyItem
{
    public GameObject relatablePropertyValuePrefab;
    private Property _property;
    public Toggle toggle;
    public override void SetPropertyData(Property prop)
    {
        _property = prop;
        //Clean Values
        CleanValues();
        
        propertyName.text = I18N.instance.getValue("^"+prop.GetName()).ToUpperInvariant() ?? prop.GetName().ToUpperInvariant();
        var possibleValues = MapUIManager.instance.GetSelectedMarker().getPropertyValue(prop.GetName());
        if (possibleValues == null)
        {
            return;
        }
        foreach (var value in possibleValues)
        {
            var go = Instantiate(relatablePropertyValuePrefab, this.transform);
            go.GetComponent<RelatablePropertyValueItem>().setValue(value);
            go.GetComponent<RelatablePropertyValueItem>().SetProperty(prop);
        }

        toggle.isOn = MapUIManager.instance.GetSelectedMarker().propertyRelationsShown(prop.GetName());

    }

    private void CleanValues()
    {
        foreach (var valueItem in GetComponentsInChildren<RelatablePropertyValueItem>())
        {
            Destroy(valueItem.gameObject);
        }
    }
    public void ToggleRelatioships(bool isOn)
    {
        if (isOn)
        {
            //Show relations with this value
            MapUIManager.instance.GetSelectedMarker().showRelations(_property.GetName());
            //Debug.LogFormat("Show relations for property {0}",_property.GetName());
        }
        else
        {
            //Hide relations with this value
            MapUIManager.instance.GetSelectedMarker().removeRelations(_property.GetName());
        }
    }
}
