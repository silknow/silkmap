﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FilterManager
{
    Dictionary<string,Filter> filterDictionary = new Dictionary<string,Filter>();
    bool activeFilter;
    bool comparationMode = Filter.OR_COMPARATION;

    public FilterManager()
    {
        this.activeFilter = false;
    }

    public void setComparationMode(bool comparationMode)
    {
        this.comparationMode = comparationMode;
    }

    public bool getComparationMode()
    {
        return this.comparationMode;
    }

    public void addFilter(string propertyName, List<string> values)
    {
        if (filterDictionary.ContainsKey(propertyName))
            updateFilter(propertyName, values);
        else
        {
            Filter filter = new Filter(propertyName, values);
            filterDictionary.Add(propertyName, filter);
        }
    }

    public void updateFilter(string propertyName, List<string> values)
    {
        if (filterDictionary.ContainsKey(propertyName))
        {
            if (values != null && values.Count > 0) 
                filterDictionary[propertyName].update(values);
            else
                removeFilter(propertyName);
        }

    }

    public void removeFilter(string propertyName)
    {
        if (filterDictionary.ContainsKey(propertyName))
        {
            filterDictionary[propertyName].remove();
            filterDictionary.Remove(propertyName);
        }

    }


    public void removeFilter(string propertyName, List<string> values)
    {
        if (filterDictionary.ContainsKey(propertyName))
        {
            filterDictionary[propertyName].RemoveValues(values);

            if (filterDictionary[propertyName].getValues().Count == 0)
                removeFilter(propertyName);
        }
    }

    /*
     * Get a List<string> with the name of the filtered properties 
     * */
    public List<string> getFilteredPropertiesName()
    {
        return filterDictionary.Keys.ToList<string>();
    }

    /**
     * Returns a List<string> with the values of a filtered property, which name is specified in the parameter
     * */
    public List<string> getFilteredPropertyValues(string propertyName)
    {
        if (getFilteredPropertiesName().Contains(propertyName))
            return filterDictionary[propertyName].getValues();
        else
            return null;
    }

    public void applyFilters(List<MapPoint> points)
    {
        Dictionary<MapPoint,int> filteredDictionary = new Dictionary<MapPoint,int>();

        foreach (MapPoint p in points)
            p.setFiltered(false);

        //Debug.Log("ACTIVOS FILTROS " + filterDictionary.Keys.Count);

        /*
        foreach (string k in getFilteredPropertiesName())
        {
            Debug.Log("La CLAVE " + k + " tiene " + filterDictionary[k]);
            foreach (string v in getFilteredPropertyValues(k))
                Debug.Log("VALOR " + v);
        }
        */
        
        foreach (MapPoint p in points) 
        {
            foreach (Filter f in filterDictionary.Values)
            {
                if (!f.checkFilterWithPoint(p, comparationMode))
                {
                    if (filteredDictionary.ContainsKey(p))
                        filteredDictionary[p]++;
                    else
                        filteredDictionary.Add(p, 1);
                }

            }
        }

        foreach (MapPoint p in filteredDictionary.Keys)
        {

            if (comparationMode == Filter.AND_COMPARATION)
                p.setFiltered(true);
            else
            {
                if (filteredDictionary[p] == filterDictionary.Keys.Count)
                    p.setFiltered(true);
            }

            /*
            if (p.getLabel().Equals("8823"))
                Debug.Log("localizado "+p.isFiltered());

            if (!p.isFiltered())
            {
                Debug.Log("pasa el filtro el punto " + p.getLabel());
            }*/
        }

        filteredDictionary.Clear();
    }



    public void resetFilters(List<MapPoint> points)
    {
        filterDictionary.Clear();
        foreach (MapPoint p in points)
            p.setFiltered(false);
    }
}
