using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
using Dummiesman;
using TMPro;
using System;
using System.Globalization;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq; // Не забудьте добавить это пространство имен

public struct objects
{
    public GameObject in_object;
    public objects(GameObject obj)
    {
        in_object = obj;
    }
}

public class neighbour
{
    public Node[] nodes;
    public GameObject lineObject;
    public neighbour(Node node1, Node node2)
    {
        if(node1 == null)
        {
            Debug.Log("node1 is null");
        }
        node1.addNodeNeighbour(this);
        node2.addNodeNeighbour(this);
        nodes = new Node[2];
        nodes[0] = node1;
        nodes[1] = node2;
        lineObject = null;
    }

    public void removeNeighbour()
    {
        nodes[0].deleteNodeNeighbor(this);
        nodes[1].deleteNodeNeighbor(this);
    }
}

public static class BoundsExtensions
{
    public static Bounds GetViewportBounds(this Camera camera, Vector3 screenPosition1, Vector3 screenPosition2)
    {
        var v1 = camera.ScreenToViewportPoint(screenPosition1);
        var v2 = camera.ScreenToViewportPoint(screenPosition2);
        var min = Vector3.Min(v1, v2);
        var max = Vector3.Max(v1, v2);

        min.z = camera.nearClipPlane;
        max.z = camera.farClipPlane;

        var bounds = new Bounds();
        bounds.SetMinMax(min, max);

        return bounds;
    }
}

public class NodeSetToolController : MonoBehaviour
{
    public GameObject canvas;
    public TMP_InputField pathString;
    public GameObject nodeObject;
    public GameObject neighObject;
    public float movementSpeed = 5f;
    public float rotationSpeed = 120f;
    public List<objects> objects = new List<objects>();
    public List<GameObject> Nodes;
    public List<neighbour> NeighboursNodes = new List<neighbour>();
    public static List<Tuple<GameObject, List<Color>>> unitSelected = new List<Tuple<GameObject, List <Color>>>(); // массив выделенных юнитов
    private int selectType = 0; // Тип выделения 0 - объекты. 1 - ноды, 2 - пути
    private Button[] selectButtons = new Button[3];
    
    private ColorBlock btnColor;
    public GUISkin skin;
    private bool selectMode;
    private Rect rect;
    private bool draw;
    //
    private Button[] moveButtons = new Button[4];
    private bool move;
    private int moveType = 0;
    public float raycastDistance = 5f; // Расстояние луча для проверки
    //
    private bool addPointsMode;
    private Button addPointsButton;
    private bool creatingNode = false; // Флаг для отслеживания состояния создания объекта
    //
    private Vector2 startPos;
    private Vector2 endPos;
    private Camera mainCamera;

    private TMP_InputField PositionCords;
    private TMP_InputField RotateCords;

    void Start()
    {
        mainCamera = Camera.main;
        pathString = canvas.transform.Find("InputPath").Find("InputField").GetComponent<TMP_InputField>();
        //Получаем все кнопки выделения
        selectButtons[0] = canvas.transform.Find("HeaderPanel").Find("SelectObjects").GetComponent<Button>();
        selectButtons[1] = canvas.transform.Find("HeaderPanel").Find("SelectNodes").GetComponent<Button>();
        selectButtons[2] = canvas.transform.Find("HeaderPanel").Find("SelectNeighbours").GetComponent<Button>();

        moveButtons[0] = canvas.transform.Find("HeaderPanel").Find("MoveHorizontal").GetComponent<Button>();
        moveButtons[1] = canvas.transform.Find("HeaderPanel").Find("MoveVertical").GetComponent<Button>();
        moveButtons[2] = canvas.transform.Find("HeaderPanel").Find("RotateAround").GetComponent<Button>();
        moveButtons[3] = canvas.transform.Find("HeaderPanel").Find("RotateAllLines").GetComponent<Button>();
        addPointsButton = canvas.transform.Find("HeaderPanel").Find("AddPointsMode").GetComponent<Button>();


        PositionCords = canvas.transform.Find("RightPanel").Find("Scroll View").Find("Viewport").Find("Content").Find("PositionField").GetComponent<TMP_InputField>();
        RotateCords = canvas.transform.Find("RightPanel").Find("Scroll View").Find("Viewport").Find("Content").Find("RotateField").GetComponent<TMP_InputField>();
        btnColor = selectButtons[0].colors;
        selectObjects(0);
        SetMoveType(0);
    }

    public void SetMoveType(int type)
    {
        for (int i = 0; i < 4; i++)
        {
            if (i == type)
            {
                ColorBlock cb = moveButtons[i].colors;
                cb.normalColor = moveButtons[i].colors.pressedColor;
                cb.highlightedColor = moveButtons[i].colors.pressedColor;
                cb.selectedColor = moveButtons[i].colors.pressedColor;
                moveButtons[i].colors = cb;
            }
            else
            {
                moveButtons[i].colors = btnColor;
            }
        }
        moveType = type;
    }
    public void selectObjects(int type)
    { 
        for (int i = 0; i < 3; i++)
        {
            if (i == type)
            {
                ColorBlock cb = selectButtons[i].colors;
                cb.normalColor = selectButtons[i].colors.pressedColor;
                cb.highlightedColor = selectButtons[i].colors.pressedColor;
                cb.selectedColor = selectButtons[i].colors.pressedColor;
                selectButtons[i].colors = cb;
            }
            else
            {
                selectButtons[i].colors = btnColor;
            }
        }
        Deselect();
        selectType = type;
    }
    public void changeMoveSpeed(Single a)
    {
        movementSpeed = (float)a;
    }
    public void changeRotateSpeed(Single a)
    {
        rotationSpeed = (float)a;
    }

    public void UpdateObjectPos()
    {
        if(unitSelected.Count == 1)
        {
            GameObject obj = unitSelected[0].Item1;
            PositionCords.text = $"{String.Format("{0:0.000}", obj.transform.position.x)}, {String.Format("{0:0.000}", obj.transform.position.y)}, {String.Format("{0:0.000}", obj.transform.position.z)}";
            RotateCords.text = $"{String.Format("{0:0.000}", obj.transform.rotation.eulerAngles.x)}, {String.Format("{0:0.000}", obj.transform.rotation.eulerAngles.y)}, {String.Format("{0:0.000}", obj.transform.rotation.eulerAngles.z)}";
        }
    }
    public void SetObjectPos(string pos)
    {
        Vector3 Objpos = StringToVector(pos);
        if (unitSelected.Count == 1)
        {
            GameObject obj = unitSelected[0].Item1;
            obj.transform.position = Objpos;
        }
    }
    public void SetObjectRot(string rot)
    {
        Vector3 Objrot = StringToVector(rot);
        if (unitSelected.Count == 1)
        {
            GameObject obj = unitSelected[0].Item1;
            obj.transform.rotation = Quaternion.Euler(Objrot);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (!addPointsMode)
            {
                if (selectMode)
                    selectMode = false;
                else
                    selectMode = true;
            }
        }
        foreach(GameObject nodeobj in Nodes)
        {
            Node node = nodeobj.transform.GetComponent<Node>();
            if (node.neighbours.Count > 0)
            {
                if (nodeobj.transform.position != node.position)
                {
                    node.position = nodeobj.transform.position;
                    NodeUpdateNeighbours(nodeobj);
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SetMoveType(0);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            SetMoveType(1);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            SetMoveType(2);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            SetMoveType(3);
        if(Input.GetKeyDown(KeyCode.Delete))
            removeSomeRubish(); //Удаляение объектов, нод и соседей
        if(Input.GetKeyDown(KeyCode.Tab))
            ToggleAddPointsMode(); //Удаляение объектов, нод и соседей
        if(Input.GetKeyDown(KeyCode.F))
            AttachSelectedNodes(); //Соединение соседних нод
    }
    public void AttachSelectedNodes()
    {
        if(selectType == 1) //Если выделены ноды
        {
            if (unitSelected.Count < 2) //Если нод выделено меньше двух 
                return;
            if(unitSelected.Count < 4) //Если нод выделено меньше 4
            {
                neighbour[] neigh = new neighbour[unitSelected.Count];
                for(int i = 0; i < unitSelected.Count; i++)
                {
                    int secondID = i + 1;
                    if (i == unitSelected.Count - 1)
                        secondID = 0;
                    Node node1 = unitSelected[i].Item1.GetComponent<Node>();
                    Node node2 = unitSelected[secondID].Item1.GetComponent<Node>();

                    // Проверяем, что ноды еще не имеют соединения между собой
                    if (!HasConnection(node1, node2))
                    {
                        neigh[i] = new neighbour(node1, node2);
                        neigh[i].lineObject = Instantiate(neighObject, unitSelected[i].Item1.transform.position, Quaternion.identity);
                        neigh[i].lineObject.transform.GetComponent<LineRenderer>().SetPosition(0, unitSelected[i].Item1.transform.position + new Vector3(0f, 0.05f, 0f));
                        neigh[i].lineObject.transform.GetComponent<LineRenderer>().SetPosition(1, unitSelected[secondID].Item1.transform.position + new Vector3(0f, 0.05f, 0f));
                        NeighboursNodes.Add(neigh[i]);
                    }
                }
            }
        }
    }

    // Метод для проверки наличия соединения между двумя нодами
    bool HasConnection(Node node1, Node node2)
    {
        foreach (neighbour neigh in node1.neighbours)
        {
            if (neigh.nodes[0] == node2 || neigh.nodes[1] == node2)
                return true;
        }
        return false;
    }
    public void ToggleAddPointsMode()
    {
        if (addPointsMode)
        {
            addPointsMode = false;
            addPointsButton.colors = btnColor;
        }
        else
        {
            addPointsMode = true;
            ColorBlock cb = addPointsButton.colors;
            cb.normalColor = addPointsButton.colors.pressedColor;
            cb.highlightedColor = addPointsButton.colors.pressedColor;
            cb.selectedColor = addPointsButton.colors.pressedColor;
            addPointsButton.colors = cb;
            Deselect();
        }
    }
    public void removeSomeRubish()
    {
        if (selectType == 2) //Если выделены соседи
        {
            if (unitSelected.Count > 0)
                DeleteSelectedNeighbour();
        }
        else if(selectType == 1) //Если выделены ноды
        {
            if (unitSelected.Count > 0)
                DeleteSelectedNode();
        }
    }
    public void DeleteSelectedNode()
    {
        List<GameObject> removeList = new List<GameObject>();
        foreach (GameObject node in Nodes)
        {
            foreach (Tuple<GameObject, List<Color>> tpl in unitSelected)
            {
                GameObject obj = tpl.Item1;
                if (node == obj)
                {
                    removeList.Add(node);
                    break;
                }
            }
        }
        foreach (GameObject node in removeList)
        {
            Node nd = node.transform.GetComponent<Node>();
            if (nd.neighbours.Count > 0)
            {
                List<neighbour> removeNigh = new List<neighbour>();
                foreach (neighbour neigh in nd.neighbours)
                    removeNigh.Add(neigh);
                foreach (neighbour neigh in removeNigh)
                    DeleteNeighbour(neigh);
            }
            DeleteNode(node);
        }
    }
    public void DeleteNode(GameObject node)
    {
        Nodes.Remove(node);
        foreach (Tuple<GameObject, List<Color>> tpl in unitSelected)
        {
            GameObject obj = tpl.Item1;
            if (node == obj)
            {
                unitSelected.Remove(tpl);
                break;
            }
        }
        Destroy(node);
    }
    public void DeleteSelectedNeighbour()
    {
        List<neighbour> removeList = new List<neighbour>();
        foreach(neighbour neigh in NeighboursNodes)
        {
            foreach(Tuple<GameObject, List<Color>> tpl in unitSelected)
            {
                GameObject obj = tpl.Item1;
                if(neigh.lineObject == obj)
                {
                    removeList.Add(neigh);
                    break;
                }
            }
        }
        foreach(neighbour neigh in removeList)
        {
            DeleteNeighbour(neigh);
        }
    }
    public void DeleteNeighbour(neighbour neigh)
    {
        neigh.removeNeighbour(); //Удаляем зависимости у нод
        Destroy(neigh.lineObject); //Удаляем объект
        NeighboursNodes.Remove(neigh); //Удаляем из списка соседей
        foreach (Tuple<GameObject, List<Color>> tpl in unitSelected)
        {
            GameObject obj = tpl.Item1;
            if (neigh.lineObject == obj)
            {
                unitSelected.Remove(tpl);
                break;
            }
        }
    }
    public void NodeUpdateNeighbours(GameObject nodeobj)
    {
        Node node = nodeobj.transform.GetComponent<Node>();
        foreach (neighbour neigh in node.neighbours)
        {
            int num = 0;
            if (neigh.nodes[1] == node)
                num = 1;
            
            neigh.lineObject.transform.GetComponent<LineRenderer>().SetPosition(num, nodeobj.transform.position + new Vector3(0f, 0.05f, 0f));
        }
    }
    // проверка, добавлен объект или нет
    bool CheckUnit(GameObject unit)
    {
        bool result = false;
        foreach (Tuple <GameObject,List<Color>> u in unitSelected)
        {
            if (u.Item1 == unit) result = true;
        }
        return result;
    }
    void Select()
    {
        if (unitSelected.Count > 0)
        {
            for (int j = 0; j < unitSelected.Count; j++)
            {
                if (selectType == 0 || selectType == 1) //Если выделяем объекты или полигоны
                {
                    MeshRenderer mr = unitSelected[j].Item1.transform.GetComponent<MeshRenderer>();
                    if (mr != null)
                    {
                        mr.material.color = Color.red;
                    }
                    foreach (Transform child in unitSelected[j].Item1.transform)
                    {
                        mr = child.GetComponent<MeshRenderer>();
                        if (mr != null)
                        {
                            mr.material.color = Color.red;
                        }
                        mr = null;
                    }
                    UpdateObjectPos();
                    // делаем что-либо с выделенными объектами
                    //Debug.Log(unitSelected[j].Item1.name);
                }
                else
                {
                    LineRenderer lr = unitSelected[j].Item1.transform.GetComponent<LineRenderer>();
                    lr.SetColors(new Color(1, 0, 0, 1), new Color(1, 0, 0, 1));
                }
            }
        }
    }
    void Deselect()
    {
        if (unitSelected.Count > 0)
        {
            for (int j = 0; j < unitSelected.Count; j++)
            {
                if (selectType == 0 || selectType == 1) //Если выделяем объекты или полигоны
                {
                    //Debug.Log("Select снят");
                    MeshRenderer mr = unitSelected[j].Item1.transform.GetComponent<MeshRenderer>();
                    List<Color> clrs = unitSelected[j].Item2;
                    if (mr != null)
                    {
                        mr.material.color = clrs[0];
                        clrs.RemoveAt(0);
                    }

                    if (unitSelected[j].Item1.transform.childCount > 0)
                    {
                        for (int i = 0; i < unitSelected[j].Item1.transform.childCount; i++)
                        {
                            mr = null;
                            mr = unitSelected[j].Item1.transform.GetChild(i).transform.GetComponent<MeshRenderer>();
                            if (mr != null)
                            {
                                mr.material.color = clrs[0];
                                clrs.RemoveAt(0);
                            }
                        }
                    }
                }
                else
                {
                    List<Color> clrs = unitSelected[j].Item2;
                    LineRenderer lr = unitSelected[j].Item1.transform.GetComponent<LineRenderer>();
                    lr.SetColors(clrs[0], clrs[1]);
                }
            }
            unitSelected.Clear();
        }
    }

    void OnGUI()
    {
        GUI.skin = skin;
        GUI.depth = 99;

        // Проверяем, находится ли указатель мыши над игровым объектом (включая интерфейсные элементы)
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (selectMode)
                {
                    if(unitSelected.Count > 0)
                        Deselect();
                    startPos = Input.mousePosition;
                    draw = true;
                }
                else if(addPointsMode)
                {
                    if (!creatingNode)
                    {
                        // Создаем Ray из позиции мыши в мировое пространство
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hit;

                        if (Physics.Raycast(ray, out hit))
                        {
                            // Получаем точку попадания луча и создаем объект Node в этой позиции
                            Vector3 nodePosition = hit.point;
                            GameObject newNode = Instantiate(nodeObject, nodePosition, Quaternion.identity);
                            Nodes.Add(newNode);
                            // Можно добавить дополнительную логику для установки иных свойств нового Node (например, имя, цвет и т.д.).
                        }
                        creatingNode = true;
                        return;
                    }
                    else
                    {
                        creatingNode = false;
                    }
                }
                else if(!selectMode && unitSelected.Count > 0 && !addPointsMode) //Движение объектов
                {
                    Vector3 mousePosition = Input.mousePosition;
                    Ray ray = mainCamera.ScreenPointToRay(mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit))
                    {
                        GameObject selectedObject = hit.collider.gameObject;
                        Tuple<GameObject, List<Color>> selectedTuple = unitSelected.Find(tuple => tuple.Item1 == selectedObject);
                        if (selectedTuple != null) //Если объект есть в списке
                            move = true;
                        else //Если объекта нет в списке
                        {
                            
                            Transform selectTransform = selectedObject.transform;
                            while (selectTransform != null)
                            {
                                // Переходим к родителю для следующей 
                                selectedTuple = unitSelected.Find(tuple => tuple.Item1 == selectTransform.gameObject);
                                if (selectedTuple != null) //Если объект есть в списке
                                {
                                    move = true;
                                    break;
                                }
                                selectTransform = selectTransform.parent;
                            }
                        }
                        
                    }

                }
            }

        }


        if (Input.GetMouseButtonUp(0))
        {
            move = false;
            draw = false;
            selectMode = false;
            Select();
        }
        if (move)
        {
            if (!addPointsMode)
            {
                if (moveType == 0) //Если движение по горизонталям
                {
                    // Получаем положение мыши по горизонтали
                    float mouseX = Input.GetAxis("Mouse X");
                    float mouseY = Input.GetAxis("Mouse Y");

                    // Получаем направление движения объектов по плоскости XZ, обнуляя Y
                    Vector3 movementDirection = new Vector3(mouseX, 0f, mouseY);

                    // Переводим направление движения из локальных координат в мировые
                    movementDirection = mainCamera.transform.TransformDirection(movementDirection);
                    movementDirection.y = 0f;
                    // Нормализуем направление, чтобы объекты двигались с одинаковой скоростью в любом направлении
                    movementDirection.Normalize();

                    // Двигаем все выделенные объекты
                    foreach (var tuple in unitSelected)
                    {
                        GameObject obj = tuple.Item1;
                        obj.transform.position += movementDirection * movementSpeed * Time.deltaTime;
                        if (selectType == 1)
                        {
                            //Debug.Log($"Двигаем объект {obj.name}");
                            // Выпускаем луч вниз из текущего объекта и игнорируем объекты на слое "Ignore Raycast"
                            Ray ray = new Ray(obj.transform.position + new Vector3(0f, 0.1f, 0f), Vector3.down);
                            Ray rayUP = new Ray(obj.transform.position + new Vector3(0f, -0.1f, 0f), Vector3.up);
                            RaycastHit[] hits;

                            // Пускаем рейкаст и получаем все столкновения
                            hits = Physics.RaycastAll(ray, raycastDistance);

                            // Проверяем, найдены ли объекты
                            if (hits.Length > 0)
                            {
                                //Debug.Log("Найдены объекты снизу");
                                float closestDistance = float.MaxValue;
                                int closestIndex = -1;

                                for (int i = 0; i < hits.Length; i++)
                                {
                                    if (hits[i].transform.tag == "IgnoreRayCast")
                                        continue;
                                    if (hits[i].collider != null && hits[i].distance < closestDistance)
                                    {
                                        closestDistance = hits[i].distance;
                                        closestIndex = i;
                                    }
                                }

                                if (closestIndex != -1)
                                {
                                    //Debug.Log($"Точка перемещена к объекту {hits[closestIndex].transform.name}");
                                    obj.transform.position = new Vector3(obj.transform.position.x, hits[closestIndex].point.y, obj.transform.position.z);
                                    // Получаем вершинный индекс ближайшей вершины к точке столкновения
                                    MeshFilter meshFilter = hits[closestIndex].collider.GetComponent<MeshFilter>();
                                    Vector3 closestVertex = Vector3.zero;
                                    if (meshFilter != null && meshFilter.sharedMesh != null)
                                    {
                                        Vector3[] vertices = meshFilter.sharedMesh.vertices;
                                        Vector3 hitPointLocal = hits[closestIndex].collider.transform.InverseTransformPoint(hits[closestIndex].point);

                                        foreach (var vertex in vertices)
                                        {
                                            float distance = Vector3.Distance(vertex, hitPointLocal);
                                            if (distance < 0.65f && distance < closestDistance)
                                            {
                                                closestDistance = distance;
                                                closestVertex = vertex;
                                            }
                                        }
                                        // Перемещаем объект к ближайшей вершине, если таковая найдена
                                        if (closestVertex != Vector3.zero)
                                        {
                                            obj.transform.position = new Vector3(obj.transform.position.x, closestVertex.y, obj.transform.position.z);
                                            Debug.Log($"Нода прикреплена");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //Debug.Log("Ищем объект снизу");
                                hits = Physics.RaycastAll(rayUP, raycastDistance);
                                if (hits.Length > 0)
                                {
                                    //Debug.Log("Найдены объекты снизу");
                                    // Ищем ближайший объект среди всех столкновений
                                    float closestDistance = float.MaxValue;
                                    int closestIndex = -1;

                                    for (int i = 0; i < hits.Length; i++)
                                    {
                                        if (hits[i].collider != null && hits[i].distance < closestDistance)
                                        {
                                            closestDistance = hits[i].distance;
                                            closestIndex = i;
                                        }
                                    }
                                    if (closestIndex != -1)
                                    {
                                        //Debug.Log($"Точка перемещена к объекту {hits[closestIndex].transform.name}");
                                        obj.transform.position = new Vector3(obj.transform.position.x, hits[closestIndex].point.y, obj.transform.position.z);
                                        // Получаем вершинный индекс ближайшей вершины к точке столкновения
                                        MeshFilter meshFilter = hits[closestIndex].collider.GetComponent<MeshFilter>();
                                        Vector3 closestVertex = Vector3.zero;
                                        if (meshFilter != null && meshFilter.sharedMesh != null)
                                        {
                                            Vector3[] vertices = meshFilter.sharedMesh.vertices;
                                            Vector3 hitPointLocal = hits[closestIndex].collider.transform.InverseTransformPoint(hits[closestIndex].point);

                                            foreach (var vertex in vertices)
                                            {
                                                float distance = Vector3.Distance(vertex, hitPointLocal);
                                                if (distance < 0.65f && distance < closestDistance)
                                                {
                                                    closestDistance = distance;
                                                    closestVertex = vertex;
                                                }
                                            }
                                            // Перемещаем объект к ближайшей вершине, если таковая найдена
                                            if (closestVertex != Vector3.zero)
                                            {
                                                obj.transform.position = new Vector3(obj.transform.position.x, closestVertex.y, obj.transform.position.z);
                                                Debug.Log($"Нода прикреплена");
                                            }
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
                else if (moveType == 1) //Если движение по вертикали
                {
                    // Получаем вертикальное положение мыши
                    float mouseY = Input.GetAxis("Mouse Y");

                    // Получаем направление движения объектов по вертикальной оси Y
                    Vector3 movementDirection = new Vector3(0f, mouseY, 0f);

                    // Двигаем все выделенные объекты
                    foreach (var tuple in unitSelected)
                    {
                        GameObject obj = tuple.Item1;
                        obj.transform.position += movementDirection * movementSpeed * Time.deltaTime;
                    }
                }
                else if (moveType == 2) //Если ротация вокруг
                {
                    // Получаем горизонтальное положение мыши
                    float mouseX = Input.GetAxis("Mouse X");

                    // Получаем величину поворота объекта по оси Y
                    float rotationAmount = mouseX * rotationSpeed * Time.deltaTime;

                    // Поворачиваем все выделенные объекты вокруг оси Y
                    foreach (var tuple in unitSelected)
                    {
                        GameObject obj = tuple.Item1;
                        obj.transform.Rotate(Vector3.up, rotationAmount);
                    }
                }
                else if (moveType == 3) //Если ротация по X и Z
                {
                    // Получаем горизонтальное и вертикальное положение мыши
                    float mouseX = Input.GetAxis("Mouse X");
                    float mouseY = Input.GetAxis("Mouse Y");

                    // Получаем величину поворота объекта по осям Y, X и Z
                    float rotationAmountY = mouseX * rotationSpeed * Time.deltaTime;
                    float rotationAmountXZ = mouseY * rotationSpeed * Time.deltaTime;

                    // Поворачиваем все выделенные объекты вокруг осей Y, X и Z
                    foreach (var tuple in unitSelected)
                    {
                        GameObject obj = tuple.Item1;
                        obj.transform.Rotate(Vector3.up, rotationAmountY);
                        obj.transform.Rotate(new Vector3(rotationAmountXZ, 0f, -rotationAmountXZ));
                    }
                }
                UpdateObjectPos();
            }
        }
        if (draw)
        {
            unitSelected.Clear();
            endPos = Input.mousePosition;
            if (startPos == endPos) return;

            rect = new Rect(Mathf.Min(endPos.x, startPos.x),
                            Screen.height - Mathf.Max(endPos.y, startPos.y),
                            Mathf.Max(endPos.x, startPos.x) - Mathf.Min(endPos.x, startPos.x),
                            Mathf.Max(endPos.y, startPos.y) - Mathf.Min(endPos.y, startPos.y)
                            );

            GUI.Box(rect, "");
            if (selectType == 0) //Если выделяем объекты
            {
                for (int j = 0; j < objects.Count; j++)
                {
                    // трансформируем позицию объекта из мирового пространства, в пространство экрана
                    Vector2 tmp = new Vector2(Camera.main.WorldToScreenPoint(objects[j].in_object.transform.position).x, Screen.height - Camera.main.WorldToScreenPoint(objects[j].in_object.transform.position).y);

                    if (rect.Contains(tmp)) // проверка, находится-ли текущий объект в рамке
                    {
                        List<Color> clrs = new List<Color>();
                        if (objects[j].in_object.transform.GetComponent<MeshRenderer>() != null)
                            clrs.Add(objects[j].in_object.transform.GetComponent<MeshRenderer>().material.color);

                        if (objects[j].in_object.transform.childCount > 0)
                        {
                            for (int i = 0; i < objects[j].in_object.transform.childCount; i++)
                            {
                                MeshRenderer mr = objects[j].in_object.transform.GetChild(i).transform.GetComponent<MeshRenderer>();
                                if (mr != null)
                                    clrs.Add(mr.material.color);
                            }
                        }
                        if (unitSelected.Count == 0)
                        {
                            unitSelected.Add(new Tuple<GameObject, List<Color>>(objects[j].in_object, clrs));
                        }
                        else if (!CheckUnit(objects[j].in_object))
                        {

                            unitSelected.Add(new Tuple<GameObject, List<Color>>(objects[j].in_object, clrs));
                        }
                    }
                }
            }
            else if(selectType == 1) //Если выделяем ноды
            {
                for (int j = 0; j < Nodes.Count; j++)
                {
                    // трансформируем позицию объекта из мирового пространства, в пространство экрана
                    Vector2 tmp = new Vector2(Camera.main.WorldToScreenPoint(Nodes[j].transform.position).x, Screen.height - Camera.main.WorldToScreenPoint(Nodes[j].transform.position).y);

                    if (rect.Contains(tmp)) // проверка, находится-ли текущий объект в рамке
                    {
                        List<Color> clrs = new List<Color>();
                        if (Nodes[j].transform.GetComponent<MeshRenderer>() != null)
                            clrs.Add(Nodes[j].transform.GetComponent<MeshRenderer>().material.color);

                        if (Nodes[j].transform.childCount > 0)
                        {
                            for (int i = 0; i < Nodes[j].transform.childCount; i++)
                            {
                                MeshRenderer mr = Nodes[j].transform.GetChild(i).transform.GetComponent<MeshRenderer>();
                                if (mr != null)
                                    clrs.Add(mr.material.color);
                            }
                        }
                        if (unitSelected.Count == 0)
                        {
                            unitSelected.Add(new Tuple<GameObject, List<Color>>(Nodes[j], clrs));
                        }
                        else if (!CheckUnit(Nodes[j]))
                        {
                            unitSelected.Add(new Tuple<GameObject, List<Color>>(Nodes[j], clrs));
                        }
                    }
                }
            }
            else if(selectType == 2) //Если выделяем соседей
            {
                for (int j = 0; j < NeighboursNodes.Count; j++)
                {
                    GameObject lineObject = NeighboursNodes[j].lineObject;
                    LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();

                    // Если у объекта есть LineRenderer
                    if (lineRenderer != null)
                    {
                        Bounds bounds = new Bounds(lineRenderer.GetPosition(0), Vector3.zero);

                        // Обходим все точки линии, чтобы определить границы объекта
                        for (int i = 0; i < lineRenderer.positionCount; i++)
                        {
                            bounds.Encapsulate(lineRenderer.GetPosition(i));
                        }

                        // Трансформируем позицию центра объекта из мирового пространства в пространство экрана
                        Vector2 tmp = new Vector2(Camera.main.WorldToScreenPoint(bounds.center).x, Screen.height - Camera.main.WorldToScreenPoint(bounds.center).y);

                        if (rect.Contains(tmp)) // Проверка, находится ли текущий объект в рамке
                        {
                            List<Color> clrs = new List<Color>();
                            // Получаем цвета из LineRenderer (для примера, можно также добавить другие свойства LineRenderer)
                            clrs.Add(lineRenderer.startColor);
                            clrs.Add(lineRenderer.endColor);

                            if (unitSelected.Count == 0)
                            {
                                unitSelected.Add(new Tuple<GameObject, List<Color>>(lineObject, clrs));
                            }
                            else if (!CheckUnit(lineObject))
                            {
                                unitSelected.Add(new Tuple<GameObject, List<Color>>(lineObject, clrs));
                            }
                        }
                    }
                }
            }         
        }
    }

    public void TurnChooseMenu()
    {
        if(canvas.transform.Find("InputPath").gameObject.activeSelf)
        {
            canvas.transform.Find("InputPath").gameObject.SetActive(false);
            pathString.text = "";
        }
        else
        {
            canvas.transform.Find("InputPath").gameObject.SetActive(true);
        }
    }
#if UNITY_EDITOR
    public void ChooseFile()
    {
        string path = EditorUtility.OpenFilePanel("�������� .obj ���� ��� �������", "", "obj");
        if (path.Length != 0)
        {
            pathString.text = path;
        }
    }
#endif

    public void LoadFile()
    {
        if (pathString.text.Length != 0) //Если путь содержит более 1 символа
        {
            if (File.Exists(pathString.text)) //Если по пути существует какой-то файл
            {
                GameObject newobject = new OBJLoader().Load(pathString.text); //Создаём новый объект и загружаем в него 3D модель
                newobject.transform.localScale = new Vector3(1f, 1f, 1f); //Устанавливаем размер объекту по умолчанию

                //Считываем полигональную сетку для записи её в отдельную стркутуру для дальнейших манипуляций
                MeshFilter meshFilter = newobject.GetComponent<MeshFilter>(); //Получаем полигональную сетку модели
                if (meshFilter != null) //Если сетка существует - создаём коллизию на её основе
                {
                    Mesh mesh = meshFilter.sharedMesh;
                    MeshCollider meshCollider = newobject.AddComponent<MeshCollider>();
                    meshCollider.sharedMesh = mesh;
                }
                
                for(int i = 0; i < newobject.transform.childCount; i++) //Проходимся по всем загруженным объектам
                {
                    meshFilter = null;
                    meshFilter = newobject.transform.GetChild(i).GetComponent<MeshFilter>(); //Получаем полигональную сетку
                    if (meshFilter != null) //Если сетка существует - создаём коллизию на её основе
                    {
                        Mesh mesh = meshFilter.sharedMesh;
                        MeshCollider meshCollider = newobject.transform.GetChild(i).gameObject.AddComponent<MeshCollider>();
                        meshCollider.sharedMesh = mesh;
                    }
                }
                objects obj = new objects(newobject);
                objects.Add(obj); //Добавляем объект в список импортированных объектов
                TurnChooseMenu(); //Закрываем панель импорта
            }
        }
    }
    public void GenerateNodesForObjects()
    {
        Debug.Log("GenerateNodesForObjects");
        foreach(Tuple <GameObject, List<Color>> tpl in unitSelected)
        {
            GameObject obj = tpl.Item1;
            for(int i = 0; i < objects.Count; i++)
            {
                if(objects[i].in_object == obj)
                {
                    Debug.Log($"Найден объект {obj.name}");
                    Vector3[] vertices; // Массив вершин
                    int[] triangles; // Двумерный массив треугольников
                    MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
                    if (meshFilter != null)
                    {
                        Mesh mesh = meshFilter.sharedMesh;
                        if (mesh != null)
                        {
                            Debug.Log($"Найден компонент MeshFilter у объекта {obj.name}");
                            Tuple<Vector3[], int[]> tuple = RemoveDuplicates(mesh);
                            vertices = tuple.Item1;
                            triangles = tuple.Item2;
                            int triangleCount = triangles.Length / 3;

                            //Создаём ноды на каждом полигоне
                            GameObject[] NodesInObject = new GameObject[vertices.Length];
                            int vertxCount = 0;
                            foreach (Vector3 coords in vertices)
                            {
                                NodesInObject[vertxCount] = Instantiate(nodeObject, coords, Quaternion.identity);
                                Nodes.Add(NodesInObject[vertxCount]);
                                vertxCount++;
                            }


                            //соединяем ноды между собой
                            for (int a = 0; a < triangleCount; a++)
                            {
                                string ints = "";
                                string neighLog = "";
                                for (int nodeID = 0; nodeID < 3; nodeID++)
                                {
                                    int secondID = nodeID + 1;
                                    if (nodeID == 3 - 1)
                                        secondID = 0;
                                    ints = $"{ints} {triangles[a * 3 + nodeID]} {triangles[a * 3 + secondID]}";

                                    neighbour neigh = new neighbour(NodesInObject[triangles[a * 3 + nodeID]].transform.GetComponent<Node>(), NodesInObject[triangles[a * 3 + secondID]].transform.GetComponent<Node>());
                                    neigh.lineObject = Instantiate(neighObject, NodesInObject[triangles[a * 3 + nodeID]].transform.position, Quaternion.identity);
                                    neigh.lineObject.transform.GetComponent<LineRenderer>().SetPosition(0, NodesInObject[triangles[a * 3 + nodeID]].transform.position + new Vector3(0f, 0.05f, 0f));
                                    neigh.lineObject.transform.GetComponent<LineRenderer>().SetPosition(1, NodesInObject[triangles[a * 3 + secondID]].transform.position + new Vector3(0f, 0.05f, 0f));
                                    NeighboursNodes.Add(neigh);
                                    neighLog = $"{neighLog} {triangles[a * 3 + nodeID]} сосед - {triangles[a * 3 + secondID]}";
                                    neighLog = $"{neighLog} {triangles[a * 3 + secondID]} сосед - {triangles[a * 3 + nodeID]}";
                                }
                                //Debug.Log(ints + "\n" + neighLog);
                            }
                        }
                    }
                    for (int child = 0; child < obj.transform.childCount; child++)
                    {
                        meshFilter = obj.transform.GetChild(child).GetComponent<MeshFilter>();
                        if (meshFilter != null)
                        {
                            Mesh mesh = meshFilter.mesh;
                            if (mesh != null)
                            {
                                
                                Debug.Log($"Найден компонент MeshFilter у дочернего объекта {obj.transform.GetChild(child).name}");
                                vertices = null;
                                triangles = null;

                                Tuple<Vector3[], int[]> tuple = RemoveDuplicates(mesh);
                                vertices = tuple.Item1;
                                triangles = tuple.Item2;
                                int triangleCount = triangles.Length / 3;
                                Debug.Log($"Объект содержит {mesh.vertices.Length} вертексов");
                                //Создаём ноды на каждом полигоне
                                GameObject[] NodesInObject = new GameObject[vertices.Length];
                                int vertxCount = 0;
                                string str = "";
                                foreach (Vector3 coords in vertices)
                                {
                                    NodesInObject[vertxCount] = Instantiate(nodeObject, coords, Quaternion.identity);
                                    Nodes.Add(NodesInObject[vertxCount]);
                                    vertxCount++;
                                }

                                //соединяем ноды между собой
                                for (int a = 0; a < triangleCount; a++)
                                {
                                    for (int nodeID = 0; nodeID < 3; nodeID++)
                                    {
                                        int secondID = nodeID + 1;
                                        if (nodeID == 3 - 1)
                                            secondID = 0;


                                        Node node1 = NodesInObject[triangles[a * 3 + nodeID]].transform.GetComponent<Node>();
                                        Node node2 = NodesInObject[triangles[a * 3 + secondID]].transform.GetComponent<Node>();
                                        // Проверяем, что ноды еще не имеют соединения между собой
                                        if (!HasConnection(node1, node2))
                                        {
                                            neighbour neigh = new neighbour(node1, node2);
                                            neigh.lineObject = Instantiate(neighObject, NodesInObject[triangles[a * 3 + nodeID]].transform.position, Quaternion.identity);
                                            neigh.lineObject.transform.GetComponent<LineRenderer>().SetPosition(0, NodesInObject[triangles[a * 3 + nodeID]].transform.position + new Vector3(0f, 0.05f, 0f));
                                            neigh.lineObject.transform.GetComponent<LineRenderer>().SetPosition(1, NodesInObject[triangles[a * 3 + secondID]].transform.position + new Vector3(0f, 0.05f, 0f));
                                            NeighboursNodes.Add(neigh);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
                }
            }
        }
    }
    public Tuple<Vector3[], int[]> RemoveDuplicates(Mesh mesh)
    {
        
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        //Debug.Log($"Изначальное число вершин {vertices.Length}, число треугольников: {triangles.Length / 3}");
        List<int> uniqueVertex = new List<int>();
        List<int> deletedVertex = new List<int>();
        for (int i = 0; i < vertices.Length; i++)
        {
            if (deletedVertex.Contains(i))
            {
                //Debug.Log($"Вершина {i} уже удалена, переходим к следующему");
                continue;
            }
            List<int> removeDoubles = new List<int>(); //Создаём список где будем хранить дублирующие вершины
            for(int a = 0; a < vertices.Length; a++) //Ищем дублирующие вершины
            {
                if (i == a)
                    continue;
                if (Vector3.Distance(vertices[i], vertices[a]) < 0.05)
                {
                    //Debug.Log($"Найдена дублирующая вершина {i} {a} {vertices[i]} {vertices[a]}");
                    deletedVertex.Add(a);
                    removeDoubles.Add(a);
                }
            }
            if (removeDoubles.Count > 0) //Если нашли дублирующие вершины
            {
                removeDoubles.Add(i); //Добавляем вершину с которой сравнивали чтобы найти минимальный индекс
                for (int j = 0; j < triangles.Length; j++)
                {
                    if (removeDoubles.Contains(triangles[j]))
                    {
                        triangles[j] = uniqueVertex.Count;
                    }
                }
                uniqueVertex.Add(i);
            }
            else
            {
                removeDoubles.Add(i); //Добавляем вершину с которой сравнивали чтобы найти минимальный индекс
                for (int j = 0; j < triangles.Length; j++)
                {
                    if (removeDoubles.Contains(triangles[j]))
                    {
                        triangles[j] = uniqueVertex.Count;
                    }
                }
                uniqueVertex.Add(i);
            }
        }
        //Debug.Log($"Осталось {uniqueVertex.Count} вершин после чистки");
        Vector3[] newVertices = new Vector3[uniqueVertex.Count];
        for(int i = 0; i < uniqueVertex.Count; i++)
        {
            newVertices[i] = vertices[uniqueVertex[i]];
        }

        /*mesh.vertices = newVertices;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();*/
        return new Tuple<Vector3[], int[]>(newVertices, triangles);
    }



    public void ExtractTrianglesFromObj(string filePath, List<float[]> vertices, List<int[]> triangles)
    {
        using (StreamReader objFile = new StreamReader(filePath))
        {
            string line;
            while ((line = objFile.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.StartsWith("v "))
                {
                    string[] vertexCoords = line.Split(' ')[1..];  // Извлечение координат вершины
                    float[] vertexArray = new float[vertexCoords.Length];
                    CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                    ci.NumberFormat.CurrencyDecimalSeparator = ".";
                    for (int i = 0; i < vertexCoords.Length; i++)
                    {
                        try
                        {
                            vertexArray[i] = float.Parse(vertexCoords[i], NumberStyles.Any, ci);
                        }
                        catch(Exception e)
                        {
                            Debug.Log($"Не удалось преобразовать строку {vertexCoords[i]} в float. {e.Message}");
                        }
                    }
                    vertices.Add(vertexArray);
                }
                else if (line.StartsWith("f "))
                {
                    string[] vertexIndices = line.Split(' ')[1..];  // Извлечение индексов вершин полигона

                    if (vertexIndices.Length == 3)  // Проверка, что полигон является треугольником
                    {
                        int[] triangle = new int[vertexIndices.Length];
                        string str = "";
                        for (int i = 0; i < vertexIndices.Length; i++)
                        {
                            string[] vertexData = vertexIndices[i].Split('/');
                            triangle[i] = int.Parse(vertexData[0]) - 1;
                            str = $"{str}, {triangle[i]}";
                        }
                        Debug.Log($"Выход: {str}");
                        triangles.Add(triangle);
                    }
                }
            }
        }
    }
    // Функция для преобразования строки в Vector3
    public Vector3 StringToVector(string input)
    {
        string[] values = input.Split(' '); // Разделяем строку по пробелу

        float x = 0f;
        float y = 0f;
        float z = 0f;

        // Проверяем количество полученных значений
        if (values.Length >= 1)
        {
            float.TryParse(values[0], out x); // Преобразуем значение x в тип float
        }
        if (values.Length >= 2)
        {
            float.TryParse(values[1], out y); // Преобразуем значение y в тип float
        }
        if (values.Length >= 3)
        {
            float.TryParse(values[2], out z); // Преобразуем значение z в тип float
        }

        return new Vector3(x, y, z); // Создаем и возвращаем новый объект Vector3
    }
}
