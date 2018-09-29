/*
* Created by Daniel Mak
*/

using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace Daniel.Tiles {
	public class GameManager : MonoBehaviour {

        #region Serialize Fields

        [Header("Grid and Tilemap")]
        [SerializeField] private Grid grid = null;
        [SerializeField] private Tilemap tilemap = null;
        [SerializeField] private Tile usableSpace = null;
        [SerializeField] private Tile unusableSpace = null;
        [SerializeField] private int tilemapHeight = 5;
        [SerializeField] private TMP_InputField borrowedInput = null;

        [Header("Time")]
        [SerializeField] private float dayLength = 0;
        [SerializeField] private Image clockFill = null;

        [Header("Capital")]
        [SerializeField] private int initialCapital = 0;

        [Header("Readonly")]
        [SerializeField] [ReadOnly] private int borrowedSpace = 0;
        [SerializeField] [ReadOnly] private Vector3Int firstUnusablePosition;
        [SerializeField] [ReadOnly] private float dayPassed = 0;

        #endregion

        #region Pubic Methods

        public void SetBorrowedSpace() {
            int newBorrowedSpace = int.Parse(borrowedInput.text);
            if (0 <= newBorrowedSpace && newBorrowedSpace <= 4 * (tilemapHeight - 1) * (tilemapHeight - 1)) {
                int deltaSize = newBorrowedSpace - borrowedSpace;
                Realloc(deltaSize);
            } else {
                borrowedInput.text = "";
            }
        }

        #endregion

        #region Unity Callback Methods

        private void Update() {
            // Debug
            if (Input.GetMouseButtonDown(0)) {
                ClickOnTile();
            }

            // Adjust camera size
            Camera.main.orthographicSize = tilemapHeight;

            // Increment time and update the clock
            dayPassed += Time.deltaTime;
            clockFill.fillAmount = dayPassed / dayLength;

            //end of day
            if (dayPassed >= dayLength) {
                Realloc(-borrowedSpace);
            }
        }

        private void OnValidate() {
            firstUnusablePosition = new Vector3Int(-tilemapHeight + 1, tilemapHeight - 2, 0);
            Camera.main.orthographicSize = tilemapHeight;
        }

        #endregion

        #region Private Methods

        private void ClickOnTile() {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 worldPoint = ray.GetPoint(-ray.origin.z / ray.direction.z);
            Vector3Int position = grid.WorldToCell(worldPoint);

            print(position);
        }

        private void BorrowFromHeap(Vector3Int position) {
            if (tilemap.GetTile(position) != null && tilemap.GetTile(position).Equals(unusableSpace)) {
                tilemap.SetTile(position, usableSpace);
            }
        }

        private void ReturnToHeap(Vector3Int position) {
            if (tilemap.GetTile(position) != null && tilemap.GetTile(position).Equals(usableSpace)) {
                tilemap.SetTile(position, unusableSpace);
            }
        }

        private void Realloc(int deltaSize) {
            int i = 0;
            int x = firstUnusablePosition.x;
            int y = firstUnusablePosition.y;
            Vector3Int location = new Vector3Int(x, y, 0); ;

            while (i < Mathf.Abs(deltaSize)) {
                //print(x.ToString() + ' ' + y.ToString());

                if (deltaSize > 0) {
                    BorrowFromHeap(location);

                    x++;
                    if (x >= tilemapHeight - 1) {
                        x = -tilemapHeight + 1;
                        y--;
                    }
                    i++;

                    location = new Vector3Int(x, y, 0);
                } else {
                    x--;
                    if (x <= -tilemapHeight) {
                        x = tilemapHeight - 2;
                        y++;
                    }
                    i++;

                    location = new Vector3Int(x, y, 0);

                    ReturnToHeap(location);
                }
            }

            firstUnusablePosition = location;
            borrowedSpace += deltaSize;
        }

        #endregion
    }
}