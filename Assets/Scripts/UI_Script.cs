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
    GameObject selectedArrow;

    TextField xRotation, zRotation, yRotation;

    TextField nameInput;

    TextField rInput, gInput, bInput;

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

        this.xRotation = rootElement.Q<TextField>("XRotation");
        this.yRotation = rootElement.Q<TextField>("YRotation");
        this.zRotation = rootElement.Q<TextField>("ZRotation");

        this.nameInput = rootElement.Q<TextField>("NameInput");
        var nameButton = rootElement.Q<Button>("ApplyName");

        this.rInput = rootElement.Q<TextField>("RInput");
        this.gInput = rootElement.Q<TextField>("GInput");
        this.bInput = rootElement.Q<TextField>("BInput");
        var colorButton = rootElement.Q<Button>("ApplyColor");

        cubeButton.clickable.clicked += onCubeButtonPressed;
        sphereButton.clickable.clicked += onSphereButtonPressed;
        planeButton.clickable.clicked += onPlaneButtonPressed;
        exportButton.clickable.clicked += onExportButtonPressed;
        scaleButton.clickable.clicked += onApplyScalePressed;
        nameButton.clickable.clicked += OnApplyNamePressed;
        colorButton.clickable.clicked += OnApplyColorPressed;

        this.selected = null;
    }

    private void onCubeButtonPressed(){
        Debug.Log("Cube Button Pressed");
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)){
            if (hit.transform != null){
                this.selected = (GameObject) Instantiate(Resources.Load("Cube"), hit.point, Quaternion.identity);
                this.selected = this.selected.transform.GetChild(0).transform.gameObject;
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
                this.selected = this.selected.transform.GetChild(0).transform.gameObject;
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
                this.selected = this.selected.transform.GetChild(0).transform.gameObject;
            }
        }
    }

    private void onExportButtonPressed(){
        Debug.Log("Export Button Pressed");
        GameObject[] cubes = GameObject.FindGameObjectsWithTag("Cube");
        GameObject[] spheres = GameObject.FindGameObjectsWithTag("Sphere");
        GameObject[] planes = GameObject.FindGameObjectsWithTag("Plane");

        GameObject[] all = new GameObject[cubes.Length + spheres.Length];
        cubes.CopyTo(all, 0);
        spheres.CopyTo(all, cubes.Length);

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
        for(int i = 0; i < all.Length; i++){
            Vector3 size = all[i].transform.GetChild(0).transform.GetComponent<Renderer>().bounds.extents;
            body body = new body();
            body.pos = all[i].transform.position.x + " " + all[i].transform.position.z + " " + all[i].transform.position.y;
            
            if(all[i].transform.GetComponent<Properties>().name != ""){
                body.name = all[i].transform.GetComponent<Properties>().name;
            }

            geom geom = new geom();
            if(i < cubes.Length){
                geom.type = "box";
            }
            else{
                geom.type = "sphere";
            }
            geom.size = size.x + " " + size.z + " " + size.y;
            geom.euler = all[i].transform.eulerAngles.x + " " + all[i].transform.eulerAngles.z + " " + all[i].transform.eulerAngles.y;
            if(all[i].transform.GetComponent<Properties>().color != ""){
                geom.rgba = all[i].transform.GetComponent<Properties>().color;
            } else {
                geom.rgba = "0 .9 0 1";
            }
            body.geom = geom;
            model.worldbody.body[i] = body;

        }
        ser.Serialize(writer, model);
        writer.Close();

        string[] arrLine = File.ReadAllLines("Model.xml");
        arrLine[1] = "<mujoco>";
        string[] newText = new string[arrLine.Length - 1];
        for(int i = 1; i < arrLine.Length; i++){
            newText[i-1] = arrLine[i];
        }
        File.WriteAllLines("Model.xml", newText);
    }

    // Update is called once per frame
    void Update()
    {
        this.OnLeftClick();
        this.MoveObject();
        this.OnBackspace();
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

                        this.xRotation.value = this.selected.transform.rotation.eulerAngles.x.ToString();
                        this.yRotation.value = this.selected.transform.rotation.eulerAngles.y.ToString();
                        this.zRotation.value = this.selected.transform.rotation.eulerAngles.z.ToString();
                    }
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
            if(lastPosition != Vector3.zero){
                int layerMask = LayerMask.GetMask("ArrowHit");
                RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity, layerMask);
                for(int i = 0; i < hits.Length; i++){
                    hit = hits[i];
                    if(hit.transform.parent.gameObject == this.selectedArrow){
                        string tag = hit.collider.gameObject.transform.parent.transform.tag;
                        Transform obj = this.selected.transform.parent;
                        if(tag=="BlueArrow"){
                            Vector3 change = lastPosition - hit.point;
                            change.y = 0;
                            change.z = 0;
                            this.lastPosition = hit.point;
                            obj.transform.position -= change;
                            this.yArrow.transform.position -= change;
                            this.xArrow.transform.position -= change;
                            this.zArrow.transform.position -= change;
                        }
                        else if(tag=="GreenArrow"){
                            Vector3 change = lastPosition - hit.point;
                            change.x = 0;
                            change.z = 0;
                            this.lastPosition = hit.point;
                            obj.transform.position -= change;
                            this.yArrow.transform.position -= change;
                            this.xArrow.transform.position -= change;
                            this.zArrow.transform.position -= change;
                        }
                        else if(tag=="RedArrow"){
                            Vector3 change = lastPosition - hit.point;
                            change.x = 0;
                            change.y = 0;
                            this.lastPosition = hit.point;
                            obj.transform.position -= change;
                            this.yArrow.transform.position -= change;
                            this.xArrow.transform.position -= change;
                            this.zArrow.transform.position -= change;
                        }
                    }
                }
            } else{
                int layerMask2 = LayerMask.GetMask("Arrow");
                if(Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask2)){
                    string tag = hit.collider.gameObject.transform.parent.transform.tag;
                    Transform obj = this.selected.transform.parent;
                    Transform arrow = hit.collider.transform.parent;
                    if(lastPosition == Vector3.zero){
                        int hitMask = LayerMask.GetMask("ArrowHit");
                        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity, hitMask);
                        this.selectedArrow = arrow.gameObject;
                        for(int i = 0; i < hits.Length; i++){
                            if(hits[i].collider.gameObject.transform.parent.transform.tag == tag){
                                this.lastPosition = hits[i].point;
                                break;
                            }
                        }
                    }
                }
            }
        } else {
            this.lastPosition = Vector3.zero;
        }
    }

    private void OnBackspace(){
        if(Input.GetKeyDown(KeyCode.Backspace)){
            Destroy(this.selected.transform.parent.gameObject);
            Destroy(this.yArrow);
            Destroy(this.zArrow);
            Destroy(this.xArrow);
        }
    }

    private void onApplyScalePressed()
    {
        Debug.Log(this.yInput.value);
        float yf = float.Parse(this.yInput.value);
        float zf = float.Parse(this.zInput.value);
        float xf = float.Parse(this.xInput.value);
        Vector3 scale = new Vector3(xf, yf, zf);

        float xR = float.Parse(this.xRotation.value);
        float yR = float.Parse(this.yRotation.value);
        float zR = float.Parse(this.zRotation.value);
        Vector3 rotation = new Vector3(xR, yR, zR);
        Debug.Log(rotation);

        this.selected.transform.parent.transform.eulerAngles = rotation;
        this.selected.transform.localScale = scale;
    }


    private void OnApplyNamePressed(){
        this.selected.transform.parent.transform.GetComponent<Properties>().name = this.nameInput.value;
    }

    private void OnApplyColorPressed(){
        float r = float.Parse(this.rInput.value);
        float g = float.Parse(this.gInput.value);
        float b = float.Parse(this.bInput.value);
        Color color = new Color(r, g, b);
        this.selected.transform.GetComponent<Renderer>().material.color = color;
        this.selected.transform.parent.transform.GetComponent<Properties>().color = r + " " + g + " " + b + " 1";
    }
}
