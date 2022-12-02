using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Xml.Serialization;
using System;
using System.IO;
using UnityEngine.EventSystems;

public class UI_Script : MonoBehaviour
{
    [SerializeField]
    private UIDocument m_UIDocument;

    GameObject selected;

    TextField yInput, xInput, zInput;

    GameObject yArrow, xArrow, zArrow;

    public Material green, red, blue;

    private Vector3 lastPosition;

    // Start is called before the first frame update
    void Start()
    {
        var rootElement = m_UIDocument.rootVisualElement;
        var cubeButton = rootElement.Q<Button>("NewCubeButton");
        var sphereButton = rootElement.Q<Button>("NewSphereButton");
        var planeButton = rootElement.Q<Button>("NewPlaneButton");
        var exportButton = rootElement.Q<Button>("ExportButton");
        var scaleButton = rootElement.Q<Button>("ScaleButton");

        this.yInput = rootElement.Q<TextField>("YInput");
        this.xInput = rootElement.Q<TextField>("XInput");
        this.zInput = rootElement.Q<TextField>("ZInput");

        cubeButton.clickable.clicked += onCubeButtonPressed;
        sphereButton.clickable.clicked += onSphereButtonPressed;
        planeButton.clickable.clicked += onPlaneButtonPressed;
        exportButton.clickable.clicked += onExportButtonPressed;
        scaleButton.clickable.clicked += onApplyScalePressed;

        this.selected = null;
    }

    private void onCubeButtonPressed(){
        Debug.Log("Cube Button Pressed");
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)){
            if (hit.transform != null){
                this.selected = (GameObject) Instantiate(Resources.Load("Cube"), hit.point, Quaternion.identity);
            }
        }
    }

    private void onSphereButtonPressed(){
        Debug.Log("Sphere Button Pressed");
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)){
            if (hit.transform != null){
                this.selected = (GameObject) Instantiate(Resources.Load("Sphere"), hit.point, Quaternion.identity);
            }
        }
    }

    private void onPlaneButtonPressed(){
        Debug.Log("Plane Button Pressed");
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)){
            if (hit.transform != null){
                this.selected = (GameObject) Instantiate(Resources.Load("Plane"), hit.point, Quaternion.identity);
            }
        }
    }

    private void onExportButtonPressed(){
        Debug.Log("Export Button Pressed");
        GameObject[] cubes = GameObject.FindGameObjectsWithTag("Cube");
        GameObject[] spheres = GameObject.FindGameObjectsWithTag("Sphere");
        GameObject[] planes = GameObject.FindGameObjectsWithTag("Plane");

        XmlSerializer ser = new XmlSerializer(typeof(mujoco));
        TextWriter writer = new StreamWriter("Model.xml");
        mujoco model = new mujoco();
        model.worldbody = new worldbody();
        light light = new light();
        light.diffuse = ".5 .5 .5";
        light.pos = "0 0 3";
        light.dir = "0 0 -1";
        model.worldbody.light = new light[1];
        model.worldbody.light[0] = light;
        model.worldbody.geom = new geom[planes.Length];
        model.worldbody.body = new body[cubes.Length + spheres.Length];

        for(int i = 0; i < planes.Length; i++){
            model.worldbody.geom[i] = new geom();
            model.worldbody.geom[i].type = "plane";
            Vector3 size = planes[i].transform.GetChild(0).transform.GetComponent<Renderer>().bounds.extents;
            model.worldbody.geom[i].size = size.x + " " + size.z + " " + size.y;
            model.worldbody.geom[i].rgba = ".9 0 0 1";
        }
        for(int i = 0; i < cubes.Length; i++){
            Vector3 size = cubes[i].transform.GetChild(0).transform.GetComponent<Renderer>().bounds.extents;
            body body = new body();
            body.pos = cubes[i].transform.position.x + " " + cubes[i].transform.position.z + " " + cubes[i].transform.position.y;

            geom geom = new geom();
            geom.type = "box";
            geom.size = size.x + " " + size.z + " " + size.y;
            geom.rgba = "0 .9 0 1";
            body.geom = geom;
            model.worldbody.body[i] = body;
        }
        ser.Serialize(writer, model);
        writer.Close();

        string[] arrLine = File.ReadAllLines("Assets/Models/Model.xml");
        arrLine[1] = "<mujoco>";
        string[] newText = new string[arrLine.Length - 1];
        for(int i = 1; i < arrLine.Length; i++){
            newText[i-1] = arrLine[i];
        }
        File.WriteAllLines("Assets/Models/Model.xml", newText);
    }

    // Update is called once per frame
    void Update()
    {
        this.OnLeftClick();
        this.MoveObject();
    }

    private void OnLeftClick()
    {
        if (Input.GetMouseButtonUp(0) && this.lastPosition == Vector3.zero){
            if (Input.mousePosition.y / Screen.height > 0.2f){
                Debug.Log("Lul");
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                int layerMask = LayerMask.GetMask("Object");
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask)){
                    string tag = hit.collider.gameObject.transform.parent.transform.tag;
                    if(tag == "Cube" || tag == "Plane" || tag == "Sphere"){
                        this.selected = hit.collider.gameObject;
                        this.yInput.value = this.selected.transform.localScale.y.ToString();
                        this.zInput.value = this.selected.transform.localScale.z.ToString();
                        this.xInput.value = this.selected.transform.localScale.x.ToString();
                    }

                    Debug.Log(this.selected.transform.parent.transform.position);
                    Debug.Log(this.selected.transform.GetComponent<Renderer>().bounds.center);
                    Destroy(this.yArrow);
                    Destroy(this.zArrow);
                    Destroy(this.xArrow);

                    Vector3 extents = this.selected.GetComponent<Renderer>().bounds.extents;
                    Vector3 center = this.selected.transform.position;

                    Vector3 bluePos = new Vector3(center.x + extents.x + 1.5f, center.y, center.z);
                    this.zArrow = (GameObject) Instantiate(Resources.Load("Arrow"), bluePos, Quaternion.identity);
                    this.zArrow.transform.tag = "BlueArrow";
                    this.zArrow.transform.GetChild(0).transform.GetComponent<Renderer>().material = this.blue;

                    Vector3 greenPos = new Vector3(center.x, center.y + extents.y + 1.5f, center.z);
                    this.yArrow = (GameObject) Instantiate(Resources.Load("Arrow"), greenPos, Quaternion.Euler(0f, 0f, 90f));
                    this.yArrow.transform.tag = "GreenArrow";
                    this.yArrow.transform.GetChild(0).transform.GetComponent<Renderer>().material = this.green;

                    Vector3 redPos = new Vector3(center.x, center.y, center.z + extents.z + 1.5f);
                    this.xArrow = (GameObject) Instantiate(Resources.Load("Arrow"), redPos, Quaternion.Euler(0f, 90f, 0f));
                    this.xArrow.transform.tag = "RedArrow";
                    this.xArrow.transform.GetChild(0).transform.GetComponent<Renderer>().material = this.red;
                }
            }
        }
    }

    private void MoveObject(){
        if(Input.GetMouseButton(0)){
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            int layerMask = LayerMask.GetMask("Arrow");
            if(Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask)){
                string tag = hit.collider.gameObject.transform.parent.transform.tag;
                Transform obj = this.selected.transform.parent;
                Debug.Log(tag);
                if(tag == "BlueArrow"){
                    if(lastPosition != Vector3.zero){
                        Vector3 change = lastPosition - hit.point;
                        change.y = 0;
                        change.z = 0;
                        this.lastPosition = hit.point;
                        obj.transform.position -= change;
                        this.yArrow.transform.position -= change;
                        this.xArrow.transform.position -= change;
                        this.zArrow.transform.position -= change;
                    } else {
                        this.lastPosition = hit.point;
                    }
                }
                if(tag == "RedArrow"){
                    if(lastPosition != Vector3.zero){
                        Vector3 change = lastPosition - hit.point;
                        change.y = 0;
                        change.x = 0;
                        this.lastPosition = hit.point;
                        obj.transform.position -= change;
                        this.yArrow.transform.position -= change;
                        this.xArrow.transform.position -= change;
                        this.zArrow.transform.position -= change;

                    } else {
                        this.lastPosition = hit.point;
                    }
                }
                if(tag == "GreenArrow"){
                    if(lastPosition != Vector3.zero){
                        Vector3 change = lastPosition - hit.point;
                        change.z = 0;
                        change.x = 0;
                        this.lastPosition = hit.point;
                        obj.transform.position -= change;
                        this.yArrow.transform.position -= change;
                        this.xArrow.transform.position -= change;
                        this.zArrow.transform.position -= change;

                    } else {
                        this.lastPosition = hit.point;
                    }
                }
            }
        } else {
            this.lastPosition = Vector3.zero;
        }
    }

    private void onApplyScalePressed()
    {
        Debug.Log(this.yInput.value);
        float yf = float.Parse(this.yInput.value);
        float zf = float.Parse(this.zInput.value);
        float xf = float.Parse(this.xInput.value);
        Vector3 scale = new Vector3(xf, yf, zf);
        this.selected.transform.localScale = scale;
    }
}
