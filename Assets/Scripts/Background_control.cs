using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    private Transform cameraTransform; //keeps track of camera position.
    public Transform[] bckground; //encompasses all the background pictures for parallax manipulation.
    private int leftMost, rightMost; //keeps track of left and rightmost background pictures.
    public int viewZone; // tolerance point for main camera. used to trigger background scrolling.
    public float backGroundLength; // length of each background image.				//Offset is 21.685612 units.

    void Start()
    {
        cameraTransform = Camera.main.transform;

        bckground = new Transform[transform.childCount]; //initialize the array.

        for (int i = 0; i < transform.childCount; i++) //sets images into indices accordingly.
        {
            bckground[i] = transform.GetChild(i);
            leftMost = 0;
            rightMost = bckground.Length - 1;
        }
    }

    void scrollLeft()
    {
        int lastRight = rightMost; //keeps track of last known rightmost index.
        bckground[rightMost].position = Vector3.right * (bckground[leftMost].position.x - backGroundLength);
        bckground[rightMost].Rotate(new Vector3(0, 180, 0));
        leftMost = lastRight;
        rightMost--;
        if (rightMost < 0)
        {
            rightMost = bckground.Length - 1;
        }
    }

    void scrollRight()
    {
        int lastLeft = leftMost;
        bckground[leftMost].position = Vector3.right * (bckground[rightMost].position.x + backGroundLength);
        bckground[leftMost].Rotate(new Vector3(0, 180, 0));
        rightMost = lastLeft;
        leftMost++;
        if (leftMost > bckground.Length - 1)
        {
            leftMost = 0;
        }
    }
    void Update()
    {
        if (cameraTransform.position.x < bckground[leftMost].position.x + viewZone)
        {
            scrollLeft();
        }
        else if (cameraTransform.position.x + 20 >= bckground[rightMost].position.x + backGroundLength - viewZone)
        {
            scrollRight();
        }
    }

}