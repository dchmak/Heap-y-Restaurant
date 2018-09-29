/*
* Created by Daniel Mak
*/

using UnityEngine;
using UnityEngine.Tilemaps;

namespace Daniel.Tiles {
	public class GameManager : MonoBehaviour {

        #region Serialize Fields

        [Header("Grid and Tilemap")]
        [SerializeField] private Grid grid = null;
        [SerializeField] private Tilemap tilemap = null;
        [SerializeField] private Tile usableSpace = null;
        [SerializeField] private Tile unusableSpace = null;

        [Header("Readonly")]
        [SerializeField] [ReadOnly] private int borrowedSpace = 0;

        #endregion

        #region Private Fields


        #endregion

        #region Unity Callback Methods

        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
                BorrowFromHeap();
            }
        }

        #endregion

        #region Private Methods

        private void BorrowFromHeap() {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 worldPoint = ray.GetPoint(-ray.origin.z / ray.direction.z);
            Vector3Int position = grid.WorldToCell(worldPoint);

            if (tilemap.GetTile(position).Equals(unusableSpace)) {
                tilemap.SetTile(position, usableSpace);
                borrowedSpace++;
            }
        }

        #endregion
    }
}