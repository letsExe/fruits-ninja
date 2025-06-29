using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;


public class SliceObject : MonoBehaviour
{
    // public Transform planeDebug;
    // public GameObject target;
    public Transform startSlicePoint;
    public Transform endSlicePoint;
    public LayerMask sliceableLayer;
    public VelocityEstimator velocityEstimator;

    public Material crossSectionMaterial;
    public float cutForce = 2000;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // if (Keyboard.current.spaceKey.wasPressedThisFrame)
        // {
        //     Slice(target);
        // }
        bool hasHit = Physics.Linecast(startSlicePoint.position, endSlicePoint.position, out RaycastHit hit, sliceableLayer);
        if (hasHit)
        {
            GameObject target = hit.transform.gameObject;
            Slice(target);
        }
    }

    public void Slice(GameObject target)
    {
        Vector3 velocity = velocityEstimator.GetVelocityEstimate();
        Vector3 planeNormal = Vector3.Cross(endSlicePoint.position - startSlicePoint.position, velocity);
        planeNormal.Normalize();

        SlicedHull hull = target.Slice(endSlicePoint.position, planeNormal);

        if (hull != null)
        {
            GameObject upperHull = hull.CreateUpperHull(target, crossSectionMaterial);
            SetupSliceComponent(upperHull);

            GameObject lowerHull = hull.CreateLowerHull(target, crossSectionMaterial);
            SetupSliceComponent(lowerHull);

            Destroy(target);

        }
    }

    public void SetupSliceComponent(GameObject sliceObject)
    {
        Rigidbody rb = sliceObject.AddComponent<Rigidbody>();
        MeshCollider collider = sliceObject.AddComponent<MeshCollider>();
        collider.convex = true;
        rb.AddExplosionForce(cutForce, sliceObject.transform.position, 1);
    }
    
}
