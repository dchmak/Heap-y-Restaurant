/*
* Created by Daniel Mak
*/

using UnityEngine;
using UnityEngine.Tilemaps;

namespace Daniel.Tiles {
	public class ClickOnTile : MonoBehaviour {

        public Grid grid;
        public Tilemap tilemap;
        public Tile usableSpace;
        public Tile unusableSpace;

        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
                BorrowFromHeap();
            }
        }

        private void BorrowFromHeap() {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 worldPoint = ray.GetPoint(-ray.origin.z / ray.direction.z);
            Vector3Int position = grid.WorldToCell(worldPoint);

            if (tilemap.GetTile(position).Equals(unusableSpace)) {
                tilemap.SetTile(position, usableSpace);
            }
        }
    }
}