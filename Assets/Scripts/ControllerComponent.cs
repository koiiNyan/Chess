using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Checkers
{
    public class ControllerComponent : MonoBehaviour
    {
        [SerializeField]
        private GameObject Camera;

        [SerializeField]
        private List<GameObject> Cells;

        [SerializeField]
        private List<GameObject> WhiteChips;

        [SerializeField]
        private List<GameObject> BlackChips;

        [SerializeField]
        private List<GameObject> CellsForChips;

        [SerializeField]
        private Material HighlightMaterial;

        [SerializeField, Tooltip("Какая сторона сейчас ходит")]
        private ColorType CurrentSide;

        private bool IsSelected;
        
        private bool NeedToOvergo;



        [SerializeField]
        private List<CellComponent> OvergoCells;


        private GameObject CurrentChip;

        [SerializeField]
        private GameObject cam;

        private bool CameraIsMoving;


        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void Awake()
        {

            for (int i = 0; i < Cells.Count; i++)
            {
                // Подписываемся на события (клетки)
                Cells[i].GetComponent<CellComponent>().OnClickEventHandler += SelectActionCellComponent;
                Cells[i].GetComponent<CellComponent>().OnFocusEventHandler += HoverAction;

                Dictionary<NeighborType, CellComponent> _neighbors = new Dictionary<NeighborType, CellComponent>();

                //Заполняем словарь соседями
                for (int j = 0; j < Cells.Count; j++)
                {
                    if (!
                        (_neighbors.ContainsKey(NeighborType.TopLeft) && _neighbors.ContainsKey(NeighborType.TopRight) 
                        && _neighbors.ContainsKey(NeighborType.BottomLeft) && _neighbors.ContainsKey(NeighborType.BottomRight))
                        )
                    {
                        // Top Left x-1 z+1
                        if (Cells[i].GetComponent<CellComponent>().transform.position.x-1 == Cells[j].GetComponent<CellComponent>().transform.position.x
                            &&
                            Cells[i].GetComponent<CellComponent>().transform.position.z+1 == Cells[j].GetComponent<CellComponent>().transform.position.z)
                        {
                            _neighbors.Add(NeighborType.TopLeft, Cells[j].GetComponent<CellComponent>());
                        }

                        // Top Right x+1 z+1
                        else if (Cells[i].GetComponent<CellComponent>().transform.position.x+1 == Cells[j].GetComponent<CellComponent>().transform.position.x
                            &&
                            Cells[i].GetComponent<CellComponent>().transform.position.z+1 == Cells[j].GetComponent<CellComponent>().transform.position.z)
                        {
                            _neighbors.Add(NeighborType.TopRight, Cells[j].GetComponent<CellComponent>());
                        }

                        // Bottom Left x-1 z-1
                        else if (Cells[i].GetComponent<CellComponent>().transform.position.x-1 == Cells[j].GetComponent<CellComponent>().transform.position.x
                            &&
                            Cells[i].GetComponent<CellComponent>().transform.position.z-1 == Cells[j].GetComponent<CellComponent>().transform.position.z)
                        {
                            _neighbors.Add(NeighborType.BottomLeft, Cells[j].GetComponent<CellComponent>());
                        }

                        // Top Right x+1 z-1
                        else if (Cells[i].GetComponent<CellComponent>().transform.position.x+1 == Cells[j].GetComponent<CellComponent>().transform.position.x
                            &&
                            Cells[i].GetComponent<CellComponent>().transform.position.z-1 == Cells[j].GetComponent<CellComponent>().transform.position.z)
                        {
                            _neighbors.Add(NeighborType.BottomRight, Cells[j].GetComponent<CellComponent>());
                        }
                    }

                }


                // заполняем словарь null
                if(_neighbors.TryGetValue(NeighborType.TopLeft, out CellComponent value) == false)
                {
                    _neighbors.Add(NeighborType.TopLeft, null);
                }
                if (_neighbors.TryGetValue(NeighborType.TopRight, out CellComponent value2) == false)
                {
                    _neighbors.Add(NeighborType.TopRight, null);
                }
                if (_neighbors.TryGetValue(NeighborType.BottomLeft, out CellComponent value3) == false)
                {
                    _neighbors.Add(NeighborType.BottomLeft, null);
                }
                if (_neighbors.TryGetValue(NeighborType.BottomRight, out CellComponent value4) == false)
                {
                    _neighbors.Add(NeighborType.BottomRight, null);
                }

                // Конфигурируем соседей для клетки
                Cells[i].GetComponent<CellComponent>().Configuration(_neighbors);





            }

            
            // Подписываемся на события (шашки). Проставяем пары шашка - клетка.
            for (int i = 0; i < WhiteChips.Count; i++)
            {
                WhiteChips[i].GetComponent<ChipComponent>().OnClickEventHandler += SelectActionChipComponent;
                WhiteChips[i].GetComponent<ChipComponent>().Pair = CellsForChips[0].GetComponent<CellComponent>();
                CellsForChips[0].GetComponent<CellComponent>().Pair = WhiteChips[i].GetComponent<ChipComponent>();
                CellsForChips.RemoveAt(0);
                WhiteChips[i].GetComponent<ChipComponent>().OnFocusEventHandler += HoverAction;
            }


            for (int i = 0; i < BlackChips.Count; i++)
            {
                BlackChips[i].GetComponent<ChipComponent>().OnClickEventHandler += SelectActionChipComponent;
                BlackChips[i].GetComponent<ChipComponent>().Pair = CellsForChips[0].GetComponent<CellComponent>();
                CellsForChips[0].GetComponent<CellComponent>().Pair = BlackChips[i].GetComponent<ChipComponent>();
                CellsForChips.RemoveAt(0);
                BlackChips[i].GetComponent<ChipComponent>().OnFocusEventHandler += HoverAction;
            }
        }


        private void SelectActionChipComponent(BaseClickComponent component)
        {
            if (component.GetType().Name == "ChipComponent")
            {
                if (component.GetColor.Equals(CurrentSide))
                {
                    IsSelected = true;
                    CurrentChip = component.gameObject;
                    var chipList = component.GetColor == 0 ? WhiteChips : BlackChips;

                    // Убираем подсветку с имеющихся шашек
                    for (int i = 0; i < chipList.Count; i++)
                    {
                        if (chipList[i].GetComponent<MeshRenderer>().material.name == "HighlightMaterial (Instance)")
                        {
                            chipList[i].GetComponent<ChipComponent>().RemoveAdditionalMaterial();
                        }
                    }

                    // Убираем подсветку с ранее выбранных клеток
                    for (int i = 0; i < Cells.Count; i++)
                    {
                        if (Cells[i].GetComponent<MeshRenderer>().material.name == "HighlightMaterial (Instance)" && !Cells[i].GetComponent<CellComponent>().Equals(component.Pair))
                        {
                            Cells[i].GetComponent<CellComponent>().RemoveAdditionalMaterial();
                        }
                    }


                    // Подсвечиваем выбранную шашку
                    component.AddAdditionalMaterial(HighlightMaterial);
                    component.GetComponent<MeshRenderer>().material = HighlightMaterial;

                    // Подсвечиваем возможные клетки
                    CellComponent currentCell = (CellComponent)component.Pair;// Клетка под шашкой
                    // Если сторона белая - нужно посвечивать Top Left и Top Right, если сторона черная - нужно подсвечивать Bottom Left и Bottom Right
                    if (component.GetColor == 0)
                    {
                        var topLeft = currentCell.GetNeighbors(NeighborType.TopLeft);
                        if (topLeft != null && topLeft.Pair == null)
                        {
                            topLeft.AddAdditionalMaterial(HighlightMaterial);
                            topLeft.GetComponent<MeshRenderer>().material = HighlightMaterial;
                        }
                        else if (topLeft != null && topLeft.Pair != null && topLeft.transform.position.z != -1 && topLeft.transform.position.x != -3.5)
                        {
                            if (topLeft.GetNeighbors(NeighborType.TopLeft).Pair == null && topLeft.Pair.GetColor != component.GetColor)
                            {
                                topLeft.GetNeighbors(NeighborType.TopLeft).AddAdditionalMaterial(HighlightMaterial);
                                topLeft.GetNeighbors(NeighborType.TopLeft).GetComponent<MeshRenderer>().material = HighlightMaterial;
                                NeedToOvergo = true;
                                OvergoCells.Add(topLeft.GetNeighbors(NeighborType.TopLeft));
                            }
                        }



                        var topRight = currentCell.GetNeighbors(NeighborType.TopRight);
                        if (topRight != null && topRight.Pair == null)
                        {
                            topRight.AddAdditionalMaterial(HighlightMaterial);
                            topRight.GetComponent<MeshRenderer>().material = HighlightMaterial;
                        }

                        else if (topRight != null && topRight.Pair != null && topRight.transform.position.z != -1 && topRight.transform.position.x != 3.5)
                        {
                            if (topRight.GetNeighbors(NeighborType.TopRight).Pair == null && topRight.Pair.GetColor != component.GetColor)
                            {
                                topRight.GetNeighbors(NeighborType.TopRight).AddAdditionalMaterial(HighlightMaterial);
                                topRight.GetNeighbors(NeighborType.TopRight).GetComponent<MeshRenderer>().material = HighlightMaterial;
                                NeedToOvergo = true;
                                OvergoCells.Add(topRight.GetNeighbors(NeighborType.TopRight));
                            }
                       }




                    }
                    else
                    {
                        var bottomLeft = currentCell.GetNeighbors(NeighborType.BottomLeft);
                        if (bottomLeft != null && bottomLeft.Pair == null)
                        {
                            bottomLeft.AddAdditionalMaterial(HighlightMaterial);
                            bottomLeft.GetComponent<MeshRenderer>().material = HighlightMaterial;
 
                        }

                        else if (bottomLeft != null && bottomLeft.Pair != null && bottomLeft.transform.position.z != -8 && bottomLeft.transform.position.x != -3.5)
                        {

                            if (bottomLeft.GetNeighbors(NeighborType.BottomLeft).Pair == null && bottomLeft.Pair.GetColor != component.GetColor)
                            {
                                bottomLeft.GetNeighbors(NeighborType.BottomLeft).AddAdditionalMaterial(HighlightMaterial);
                                bottomLeft.GetNeighbors(NeighborType.BottomLeft).GetComponent<MeshRenderer>().material = HighlightMaterial;
                                NeedToOvergo = true;
                                OvergoCells.Add(bottomLeft.GetNeighbors(NeighborType.BottomLeft));
                            }
                        }



                        var bottomRight = currentCell.GetNeighbors(NeighborType.BottomRight);
                        if (bottomRight != null && bottomRight.Pair == null)
                        {
                            bottomRight.AddAdditionalMaterial(HighlightMaterial);
                            bottomRight.GetComponent<MeshRenderer>().material = HighlightMaterial;
                        }
                        else if (bottomRight != null && bottomRight.Pair != null && bottomRight.transform.position.z != -8 && bottomRight.transform.position.x != 3.5)
                        {


                            if (bottomRight.GetNeighbors(NeighborType.BottomRight).Pair == null && bottomRight.Pair.GetColor != component.GetColor)
                            {
                                bottomRight.GetNeighbors(NeighborType.BottomRight).AddAdditionalMaterial(HighlightMaterial);
                                bottomRight.GetNeighbors(NeighborType.BottomRight).GetComponent<MeshRenderer>().material = HighlightMaterial;
                                NeedToOvergo = true;
                                OvergoCells.Add(bottomRight.GetNeighbors(NeighborType.BottomRight));
                            }
                        }





                    }
                }
                else
                {
                    Debug.Log("Now is " + CurrentSide + " turn");
                }

            }


        }

        private void HoverAction(CellComponent component, bool isSelect)
        {
            // Клетки, с которых не нужно снимать подсветку (на которые можем ходить)
            if (IsSelected)
            {
                CellComponent CurrentChipCellComponent = (CellComponent)CurrentChip.GetComponent<BaseClickComponent>().Pair;

                CellComponent FirstCellComponent;
                CellComponent SecondCellComponent;

                    FirstCellComponent = CurrentSide == 0 ? CurrentChipCellComponent.GetNeighbors(NeighborType.TopLeft) :
                        CurrentChipCellComponent.GetNeighbors(NeighborType.BottomLeft);
                    SecondCellComponent = CurrentSide == 0 ? CurrentChipCellComponent.GetNeighbors(NeighborType.TopRight) :
                        CurrentChipCellComponent.GetNeighbors(NeighborType.BottomRight);


                if (!component.Equals(FirstCellComponent) && !component.Equals(SecondCellComponent) && !OvergoCells.Contains(component))
                {
                    if (isSelect)
                    {
                        component.AddAdditionalMaterial(HighlightMaterial);
                        component.GetComponent<MeshRenderer>().material = HighlightMaterial;
                    }
                    else
                    {
                        component.RemoveAdditionalMaterial();
                    }
                }
            }

            else
            {
                if (isSelect)
                {
                    component.AddAdditionalMaterial(HighlightMaterial);
                    component.GetComponent<MeshRenderer>().material = HighlightMaterial;
                }
                else
                {
                    component.RemoveAdditionalMaterial();
                }
            }
            
        }



        private void SelectActionCellComponent(BaseClickComponent component)
        {
            /* Ищем подсвеченную шашку. Для белых - на клетку можно ходить, если она x-1, z+1  / x+1, z+1 .
             Для черных - если она x-1, z-1 / x+1, z-1
            И pair = null */

            if (CurrentChip && IsSelected && component.Pair == null)
            {
                CellComponent CurrentChipCellComponent = (CellComponent)CurrentChip.GetComponent<BaseClickComponent>().Pair;

                CellComponent FirstCellComponent = CurrentSide == 0 ? CurrentChipCellComponent.GetNeighbors(NeighborType.TopLeft) :
                CurrentChipCellComponent.GetNeighbors(NeighborType.BottomLeft);
                CellComponent SecondCellComponent = CurrentSide == 0 ? CurrentChipCellComponent.GetNeighbors(NeighborType.TopRight) :
                CurrentChipCellComponent.GetNeighbors(NeighborType.BottomRight);

                if (component.Equals(FirstCellComponent) || component.Equals(SecondCellComponent) || OvergoCells.Contains((CellComponent)component))
                {
                    //Debug.Log("Can go");
                    CurrentChip.GetComponent<ChipComponent>().Pair.RemoveAdditionalMaterial();
                    var _cor = StartCoroutine(MoveChip(CurrentChip, component.transform.position.x, component.transform.position.z));
                    IsSelected = false;
                    CurrentChip.GetComponent<ChipComponent>().RemoveAdditionalMaterial();
                    if (FirstCellComponent != null) FirstCellComponent.RemoveAdditionalMaterial();
                    if (SecondCellComponent != null) SecondCellComponent.RemoveAdditionalMaterial();
                    StopCoroutine(_cor);
                    CheckIfWin(CurrentChip.GetComponent<ChipComponent>().GetColor, CurrentChip.transform.position.z);

                    ChangePairs(CurrentChip.GetComponent<ChipComponent>());
                    StartCoroutine(MoveCamera());
                    CurrentSide = CurrentSide == ColorType.White ? ColorType.Black : ColorType.White;
                    OvergoCells.Clear();
                }
                else
                {
                    //Debug.Log("CAN NOT go");
                }
            }

            else Debug.Log("You did not choose chip");


        }




        private IEnumerator MoveChip(GameObject chip, float x, float z)
        {

            float diffX = (chip.transform.position.x - x) * -1;
            float diffZ = (chip.transform.position.z - z) * -1;
            if (NeedToOvergo)
            {
                
                DestroyChip(chip, chip.GetComponent<BaseClickComponent>().GetColor, diffX, diffZ);
            }

            yield return chip.transform.position += new Vector3(diffX, 0, diffZ);



            NeedToOvergo = false;
        }

        private void ChangePairs (BaseClickComponent chip)
        {
            chip.Pair.Pair = null;

            for (int i=0; i< Cells.Count; i++)
            {
                
                if (chip.gameObject.transform.position.x == Cells[i].GetComponent<CellComponent>().transform.position.x
                    &&
                    chip.gameObject.transform.position.z == Cells[i].GetComponent<CellComponent>().transform.position.z)
                {
                    chip.Pair = Cells[i].GetComponent<CellComponent>();
                    chip.Pair.Pair = chip;
                    break;
                }
            }
        }

        private void DestroyChip(GameObject chip, ColorType color, float diffX, float diffZ)
        {

            diffX = (diffX / 2) * -1;
            diffZ = (diffZ / 2) * -1;
            var chipList = color == 0 ? BlackChips : WhiteChips;
            for (int i=0; i< chipList.Count; i++)
            {
                if (chipList[i].transform.position.x + diffX == chip.transform.position.x
                    && chipList[i].transform.position.z + diffZ == chip.transform.position.z
                    )
                {
                    
                    Destroy(chipList[i]);
                    chipList.RemoveAt(i);
                }

            }
 

        }

        private void CheckIfWin(ColorType color, float z)
        {
            if ((BlackChips.Count == 0 || color == ColorType.White && z == -1) ||
                (WhiteChips.Count == 0 || color == ColorType.Black && z == -8))
            {
                Debug.Log("=====================\n" + color + " wins!\n=====================");
                UnityEditor.EditorApplication.isPaused = true;
            }


        }



        private IEnumerator MoveCamera()
        {
            var NewPosition = CurrentSide == ColorType.White  ? new Vector3(0, 5, 0.2f) : new Vector3(0, 5, -10);
            var NewRotation = CurrentSide == ColorType.White ? new Vector3(53, 190, 12) : new Vector3(45, 3, 3);
            var _targetRotation = Quaternion.Euler(NewRotation);

            var currentTime = 0f;
            var time = 1f;
            while (currentTime < time)
            {
                cam.transform.position = Vector3.Lerp(cam.transform.position, NewPosition, 1 - (time - currentTime) / time);
                cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, _targetRotation, 1 - (time - currentTime) / time);
                currentTime += Time.deltaTime;
                yield return null;
            }
            transform.position = NewPosition;
            transform.rotation = _targetRotation;
        }
    }

}