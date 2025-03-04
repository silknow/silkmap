﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class MapPoint 
{
    public const short TWO_DIMENSION = 2;
    public const short THREE_DIMENSION = 3;
    public static int lastId = 0;
    protected short dimension = TWO_DIMENSION;

    public bool clusteredLevel = false;

    protected GameObject stackedGameObject = null;

    protected Dictionary<string, Vector2> positionValues = new Dictionary<string, Vector2>();

    public bool stacked = false;

    protected Dictionary<string, int> relatedData = new Dictionary<string, int>();

    protected bool knownLocation = true;


    // x,y,z position in map coordinates
    protected float x;
    protected float y;

    // Dsisplay or not display
    protected bool hidden = false;

    // Associated uri to the point (if exists)
    protected string uri = "";

    // Label of the point (if exists)
    protected string label = "";

    // Class/Category associated to the point
    protected string category = "";

    // The point exists in time from [from, to] years
    protected int from;
    protected int to;

    protected int id;

    public bool groupPoint = false;

    public GridCluster isInGroupPoint;

    public bool multipleLocations = false;

    // If filtered is active, the point must no be displayed
    protected bool filtered=false;

    // MapPoints related with this point
    protected List<RelationShip> relations = new List<RelationShip>();

    // Clusters where this point is related to
    protected List<GridCluster> clusterList = new List<GridCluster>();
    protected List<MapPoint> clusteredPointList=new List<MapPoint>();
    
    protected Dictionary<string, List<string>> propertyValues = new Dictionary<string, List<string>>();

    protected Dictionary<string, RelationShip> relationsPerProperty = new Dictionary<string, RelationShip>();

    // If the map point is a cluster or a data point.
    protected bool cluster = false;

    // cluster represented by this point
    GridCluster gridCluster;

    protected Map map;
    public MapPoint(float x, float y)
    {
        this.x = x;
        this.y = y;
        this.id = lastId;
        lastId++;
    }

    public int getId()
    {
        return id;
    }

    public void setMap(Map map)
    {
        this.map = map;
    }

    public Map getMap()
    {
        return map;
    }

    public bool isMultipleLocations()
    {
        return multipleLocations;
    }

    public void setMultipleLocations(bool multiple)
    {
        this.multipleLocations = multiple;
    }

    public GridCluster getGroupPointCluster()
    {
        return isInGroupPoint;
    }

    public bool isKnownLocation()
    {
        return knownLocation;
    }

    public void setKnownLocation(bool known)
    {
        this.knownLocation = known;        
    }

    public bool isVisible()
    {
        return !hidden;
    }


    public bool isGroupPoint()
    {
        /*
        if (isCluster() && getClusteredPoints()!=null && getClusteredPoints().Count>0)
            return getClusteredPoints()[0].groupPoint;
        else
            return false;*/
        if (groupPoint)
            return true;
        else
        {
            if (this.getGridCluster() != null)
                return this.getGridCluster().isGroupPoints();
            else
                return false;
        }
        
    }

    public void setGroupPoint(bool groupPoint)
    {
        this.groupPoint = groupPoint;
    }

    public void setStacked(bool status)
    {
        this.stacked = status;

        if (!stacked && stackedGameObject != null)
            MonoBehaviour.Destroy(stackedGameObject);
    }

    public void setRelatedDataFor(string propertyName, int numMatchs)
    {
        if (relatedData.ContainsKey(propertyName))
            relatedData[propertyName] = numMatchs;
        else
            relatedData.Add(propertyName, numMatchs);
    }


    public void addPositionValue(string positionName, Vector2 values)
    {
        positionValues.Add(positionName, values);
    }

    public void setStackedGameObject(GameObject gameObject)
    {
        this.stackedGameObject = gameObject;
        setStacked(true);
    }

    public GameObject getStackedGameObject()
    {
        return this.stackedGameObject;
    }

    public void activePosition(string name)
    {
        this.x = positionValues[name].x;
        this.y = positionValues[name].y;

        graphicUpdatePosition();
    }

    public virtual void graphicUpdatePosition()
    {
    }

    public bool isStacked()
    {
        return this.stacked;
    }

    /**
     * returns true is same values of propertyName are in the values List
     * */
    public bool sameValuesAtProperty(string propertyName, List<string> values)
    {
        bool sameValue = false;

        List<string> valuesOfPoint = getPropertyValue(propertyName);

        if (valuesOfPoint.Any(x => values.Any(y => y.Equals(x))))
            sameValue = true;

        return sameValue;
    }

    public void SetPropertyValue(string propertyName, List<string> values)
    {
        
        if (!propertyValues.ContainsKey(propertyName))
            propertyValues.Add(propertyName, values);
        else
            propertyValues[propertyName] = values;
    }

    public List<string> getPropertyValue(string name)
    {
        if (propertyValues.ContainsKey(name))
            return propertyValues[name];
        else
            return new List<string>();
    }

    public void reset()
    {
        relations.Clear();
        clusterList.Clear();

        graphicReset();
        //relations.RemoveRange(0, relations.Count);
        //clusterList.RemoveRange(0, clusterList.Count);
    }

    public virtual void graphicReset()
    {

    }

    public Vector2 getVector2()
    {
        return new Vector2(this.x, this.y);
    }


    public short getDimension()
    {
        return dimension;
    } 


    public void setDimension(short dimension)
    {
        if (this.dimension!=dimension)
        {
            this.dimension = dimension;
            if (!hidden)
            {
                hide();
                show();
            }
        }
    }
    
    public string getLabel()
    {
        return label;
    }

    public void setLabel(string label)
    {
        this.label = label;
        updateGraphicLabel();
    }

    public string getCategory()
    {
        return this.category;
    }

    public void setCategory(string category)
    {
        this.category = category;
    }
 
    public string getURI()
    {
        return uri;
    }

    public void setURI(string URI)
    {
        this.uri = URI;
    }

    public List<MapPoint> getClusteredPoints()
    {
        if (isCluster() || isGroupPoint())
        {
            return this.clusteredPointList;
        }
        else
            return null;
    }

    public List<MapPoint> getClusteredPointsNoFiltered()
    {
        if (isCluster() || isGroupPoint())
        {
            List<MapPoint> pointNF = new List<MapPoint>();
            foreach (MapPoint p in this.clusteredPointList)
                if (!p.isFiltered())
                    pointNF.Add(p);
            return pointNF;
        }
        else
            return null;
    }

    public void setClusteredPoints(List <MapPoint> clusteredPoints)
    {
        this.clusteredPointList = clusteredPoints;
    }

    /**
     * Function that returns if this point has a relation named <name> with the point <point>
     * @return true if the relation exist, false otherwise
     * */
    public bool hasRelationShipWith(MapPoint point, string name)
    {
        bool existsRelationship = false;
        int i = 0;
     
        while (!existsRelationship && i<relations.Count)
        {
            existsRelationship = (relations[i].isRelatedTo(point) && relations[i].getName().Equals(name));
            i++;
        }

        return existsRelationship;

    }    

    /**
     * Function that adds a <inverse> RelationShip of this point with another <point>
     * @returns the added Relationship instance
     * */
    public RelationShip addInverseRelationWith(MapPoint point, string inverse)
    {
        RelationShip relation = new RelationShip(this, point, "");
        relation.setInverse(inverse);       
        this.relations.Add(relation);

        return relation;
    }

    /**
     * Function that add a RelationShip named <name> fromt this point with <point>
     * @returns the added RelationShip instance
     * */
    public RelationShip addRelationWith(MapPoint point, string name)
    {
        RelationShip relation = null;

        if (!this.hasRelationShipWith(point,name))
        {
            relation = new RelationShip(this, point, name);
            this.relations.Add(relation);
            RelationShip inverseRelation = point.addInverseRelationWith(this, name);
            inverseRelation.hide();
        }

        return relation;
    }

    public List<RelationShip> getRelations()
    {
        return this.relations;
    }

    public void addCluster(GridCluster cluster)
    {
        this.clusterList.Add(cluster);
        if (cluster.getLevel() == 1)
            Debug.Log("Se ha añadido un cluster de nivel 1");
    }

    public void removeCluster(GridCluster cluster)
    {
        this.clusterList.Remove(cluster);
    }

    public List<GridCluster> getRelatedClusters()
    {
        return this.clusterList;
    }

    public void setXY(float x, float y)
    {
        this.x = x;
        this.y = y;

        updateGraphicsCoordinates();
    }

    public float getX()
    {
        return x;
    }

    public float getY()
    {
        return y;
    }

    public void show()
    {

        if (filtered || groupPoint)
        {
            hide();
            return;
        }

  
        if (hidden)
        {
            hidden = false;
            graphicShow();
            /*
            if (isCluster())
                gridCluster.showRelations();
            else
                showAllRelations();*/
            if (!isCluster())
                showAllRelations();
            else
                this.getGridCluster().showRelations();
        }
    }


    public void showAllRelations()
    {
        foreach (string property in relationsPerProperty.Keys)
            showRelations(property);
    }

    public void hideAllRelations()
    {
        foreach (string property in relationsPerProperty.Keys)       
            hideRelations(property);
    }

    public void removeAllRelations()
    {
        foreach (string property in relationsPerProperty.Keys)
            removeRelations(property);
    }

    public void removeRelations(string propertyName)
    {
        RelationShip relation;

        if (relationsPerProperty.ContainsKey(propertyName))
        {
            relation = this.relationsPerProperty[propertyName];
            relation.clear();
            relationsPerProperty.Remove(propertyName);

            removeGraphicRelations(propertyName);
        }

        if (relationsPerProperty.Keys.Count == 0)
        {
            map.removePointWithRelation(this);
            this.getDirectCluster().removeRelationsPerPoint(this);
        }
    }

    public void hideRelations(string propertyName)
    {
        RelationShip relation;

        if (relationsPerProperty.ContainsKey(propertyName))
        {
            relation = this.relationsPerProperty[propertyName];
            relation.hide();

            //relation.clear();
            //relationsPerProperty.Remove(propertyName);

            updateGraphicRelations(propertyName,false);

            //this.getDirectCluster().hideRelationsPerPoint(this);
        }

        
        //if (relationsPerProperty.Keys.Count == 0)
        //    map.removePointWithRelation(this);
    }

    public bool propertyRelationsShown(string propertyName)
    {
        return relationsPerProperty.ContainsKey(propertyName);
    }

    protected GridCluster getDirectCluster()
    {        
        foreach (GridCluster cluster in clusterList)        
            if (cluster.getLevel() == 1)
                return cluster;        

        return null;
    }

    protected List<GridCluster> clustersOfPoints(List<MapPoint> pointListToCheck)
    {
        List<GridCluster> clustersRelated = new List<GridCluster>();

        foreach (MapPoint p in pointListToCheck)
        {
            GridCluster directCluster = p.getDirectCluster();

            if (!clustersRelated.Contains(directCluster))
                clustersRelated.Add(directCluster);
        }

        return clustersRelated;
    }

    public void showRelations(string propertyName)
    {
        RelationShip relation;

        //Debug.Log("Show relations for property " + propertyName);
        
        // Check if relations are previously processed
        if (!relationsPerProperty.ContainsKey(propertyName))
        {
            // Get the points with the same values in propertyName
            List<MapPoint> relatedPoints = map.poinstWithValuesAtProperty(propertyName, getPropertyValue(propertyName));

            relatedPoints.Remove(this);

            relation = new RelationShip(this, relatedPoints, propertyName);

            relationsPerProperty.Add(propertyName, relation);

            List<GridCluster> relatedClusters = clustersOfPoints(relatedPoints);
            if (this.getDirectCluster() == null)
                Debug.Log("El cluster directo es null");
            else
                Debug.Log("EL cluster directo es " + this.getDirectCluster());
            this.getDirectCluster().addRelationsPerPoint(this, relatedClusters);
            //setClusterRelations(this.getDirectCluster(), relatedClusters);
            map.addPointWithRelation(this);
        }

        relation = this.relationsPerProperty[propertyName];
        relation.update();

        updateGraphicRelations(propertyName,true);

        

        //this.getDirectCluster().showRelationsPerPoint(this);
    }

    /*
    protected void setClusterRelations(GridCluster cluster, List<GridCluster> relatedClusters)
    {
        cluster.addRelationPerPoint(this, relatedClusters);
    }*/

    public void hide()
    {
        //if (getLabel().Equals("8824"))
          //  Debug.Log("localizado " + isFiltered());

        if (!hidden)
        {
            hidden = true;
            graphicHide();

            /*
            if (isCluster())
                gridCluster.hideRelations();
            else
            {
                if (isFiltered())
                    hideAllRelations();
            }
            */

            //if (!isCluster() && isFiltered())
            //  hideAllRelations();

            hideAllRelations();

            if (!isCluster())
                this.showClusterRelations();
            else
            {
                this.getGridCluster().hideRelations();
                if (this.getGridCluster().getParent() != null)
                {
                    /*if (this.map.getLevel() != this.getGridCluster().getLevel())
                    {
                        //Debug.Log("El nivel del mapa es " + this.map.getLevel());
                        //Debug.Log("El nivel del cluster es " + this.getGridCluster().getLevel());
                    }*/
                    if (this.map.getLevel()<this.getGridCluster().getLevel())
                        this.getGridCluster().getParent().showRelations();
                }
            }
        }
    }

    public void showClusterRelations()
    {
        if (this.clusterList!=null)
            for (int i=0;i<this.clusterList.Count;i++)
                if (this.clusterList[i].getLevel() == this.map.getLevel()-1)
                {
                    if (this.getURI().Equals("http://data.silknow.org/object/a1f06a7e-243f-3ca5-b2c3-0970305276de"))
                    {
                       // Debug.Log("Este punto es " + this.getX() + "," + this.getY());
                       // Debug.Log("El centro  es " + this.clusterList[i].getCenter().getX() + "," + this.clusterList[i].getCenter().getY());
                    }
                    if (this.map.getLevel() < this.clusterList[i].getLevel())
                        this.clusterList[i].showRelations();
                    return;
                }
            
    }

    public void setFiltered(bool filtered)
    {
        if (this.filtered != filtered)
        {
            this.filtered = filtered;

            if (this.filtered)
            {
                //if (this.clusterList.Count > 1)
                //   Debug.Log("Hay clusters en la lista");


                foreach (GridCluster gCluster in this.clusterList)
                    gCluster.addFilteredPoint();


                //if (isInGroupPoint != null)
                  //  isInGroupPoint.addFilteredPoint();
            }
            else
            {
                foreach (GridCluster gCluster in this.clusterList)
                    gCluster.removeFilteredPoint();

                //if (isInGroupPoint != null)
                 //   isInGroupPoint.removeFilteredPoint();
            }
        }

    }


    public int getNumObjectsWithProperty(string propertyName)
    {
        if (relatedData.ContainsKey(propertyName))
            return relatedData[propertyName];
        else
            return 0;
    }

    public bool isFiltered()
    {
        return filtered;
    }


    protected virtual void graphicHide()
    {

    }

    protected virtual void graphicShow()
    {

    }

    protected virtual void updateGraphicLabel()
    {

    }

    protected virtual void updateGraphicsCoordinates()
    {

    }

    protected virtual void updateGraphicRelations(string propertyName, bool show)
    {
    }

    protected virtual void updateGraphicRelations(MapPoint point, bool show)
    {

    }

    public void showClusterRelations(MapPoint point)
    {
        updateGraphicRelations(point, true);
    }

    public void hideClusterRelations(MapPoint point)
    {
        updateGraphicRelations(point, false);
    }

    public virtual void removeGraphicRelations(string propertyName)
    {
    }

    public virtual void removeGraphicRelations(MapPoint point)
    {

    }

    public void removeClusterRelations(MapPoint point)
    {

    }



    public void setFrom(int from)
    {
        this.from = from;
    }

    public int getFrom()
    {
        return this.from;
    }

    public void setTo(int to)
    {
        this.to = to;
    }

    public int getTo()
    {
        return this.to;
    }

    public bool isCluster()
    {
        return this.cluster;
    }

    public void setCluster(bool cluster)
    {
        this.cluster = cluster;
    }

    public void setGridCluster(GridCluster gCluster)
    {
        this.gridCluster = gCluster;
        setCluster(true);
        
    }

    public GridCluster getGridCluster()
    {
        return this.gridCluster;
    }
}
