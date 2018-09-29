/*
* Created by Daniel Mak
*/

using Daniel.Event;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    [System.Serializable]
    private class Customer {
        // in queue
        public float patient;
        public float timeInQueue = 0;

        // have space
        public int spending;
        public float stayTime;
        public float timeHaveSpace = 0;

        public Customer(float patient, int spending, float stayTime) {
            this.patient = patient;
            this.spending = spending;
            this.stayTime = stayTime;
        }
    }

    #region Serialize Fields

    [Header("Grid and Tilemap")]
    [SerializeField] private Grid grid = null;
    [SerializeField] private Tilemap tilemap = null;
    [SerializeField] private Tile usableSpace = null;
    [SerializeField] private Tile unusableSpace = null;
    [SerializeField] private Tile OccupiedSpace = null;
    [SerializeField] private int tilemapHeight = 5;
    [SerializeField] private TMP_InputField borrowedInput = null;
    [SerializeField] private float allocTime = 0;

    [Header("Time")]
    [SerializeField] private float dayLength = 0;
    [SerializeField] private Image clockFill = null;
    [SerializeField] private TextMeshProUGUI annoucer = null;

    [Header("Capital")]
    [SerializeField] private int initialCapital = 0;
    [SerializeField] private TextMeshProUGUI capitalText = null;
    [SerializeField] private int costPerSpacePerSecond = 0;
    [SerializeField] private int kickPenalty = 1;

    [Header("Rating")]
    [SerializeField] private Image starBarFill = null;
    [SerializeField] [Range(0, 5)] private float successRating = 0;
    [SerializeField] [Range(0, 5)] private float impatientPenalty = 0;
    [SerializeField] [Range(0, 5)] private float annoyingPenalty = 0;

    [Header("Queue")]
    [SerializeField] private Image[] customerIcons = null;

    [Header("Events")]
    [SerializeField] private bool dummy = false;
    [SerializeField] private CustomEvent[] events = null;

    [Header("Readonly")]
    [SerializeField] [ReadOnly] private int borrowedSpace = 0;
    [SerializeField] [ReadOnly] private Vector3Int firstUnusablePosition;
    [SerializeField] [ReadOnly] private float dayPassed = 0;
    [SerializeField] [ReadOnly] private int capital = 0;
    [SerializeField] [ReadOnly] private float star = 5;
    [SerializeField] [ReadOnly] private bool isReallocing = false;
    [SerializeField] [ReadOnly] private bool operational = true;
    [SerializeField] [ReadOnly] private CustomEvent currentEvent = null;
    [SerializeField] [ReadOnly] private Vector2 timeBetweenNewcomerRange = Vector2.zero;
    [SerializeField] [ReadOnly] private Vector2 patientRange = Vector2.zero;
    [SerializeField] [ReadOnly] private Vector2Int spendingRange = Vector2Int.zero;
    [SerializeField] [ReadOnly] private Vector2 stayTimeRange = Vector2.zero;

    #endregion

    #region Private Fields

    private List<Customer> queue = new List<Customer>();
    private Dictionary<Vector3Int, Customer> haveSpaceCustomers = new Dictionary<Vector3Int, Customer>();

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

    public void StartOfDay() {
        StartCoroutine(ChargeSpaceCost());
        StartCoroutine(RandomNewcomer());
        operational = true;
    }

    #endregion

    #region Unity Callback Methods

    private void Start() {
        StartCoroutine(ChargeSpaceCost());
        StartCoroutine(RandomNewcomer());
        annoucer.text = "Another Day to feed.\n" + currentEvent.stringToBeAnnounced;
        annoucer.gameObject.SetActive(true);

        star = 5;
    }

    private void Update() {
        // Adjust camera size
        Camera.main.orthographicSize = tilemapHeight;

        if (operational) {
            Impatience();
            VisualisePatient();
            IncrementTimeHaveSpace();

            // Assign space for customer
            if (queue.Count > 0 && Input.GetMouseButtonDown(0)) {
                Vector3Int position = ClickOnTile();
                TileBase tileBase = tilemap.GetTile(position);

                if (tileBase != null && tileBase.Equals(OccupiedSpace)) {
                    star -= annoyingPenalty;
                } else {
                    AssignSpace(position);
                }
            }

            // Increment time and update the clock
            dayPassed += Time.deltaTime;
            clockFill.fillAmount = dayPassed / dayLength;

            //end of day
            if (dayPassed >= dayLength) {
                EndOfDay();
            }
        } 
    }    

    private void LateUpdate() {
        PrintCapital();
        starBarFill.fillAmount = star / 5;

        // Lose
        if (capital <= 0 || star <= 0) {
            SceneManager.LoadScene(1);
        }
    }

    private void OnValidate() {
        borrowedSpace = 0;
        firstUnusablePosition = new Vector3Int(-tilemapHeight + 1, tilemapHeight - 2, 0);
        dayPassed = 0;
        capital = 0;
        star = 5;
        isReallocing = false;
        operational = false;

        if (events != null && events.Length > 0) {
            currentEvent = events[0];
            ApplyEvent();
        }

        capital = initialCapital;
        if (capitalText != null) {
            PrintCapital();
        }

        Camera.main.orthographicSize = tilemapHeight;
    }

    #endregion

    #region Private Methods

    private Vector3Int ClickOnTile() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 worldPoint = ray.GetPoint(-ray.origin.z / ray.direction.z);
        return grid.WorldToCell(worldPoint);
    }

    private void BorrowFromHeap(Vector3Int position) {
        TileBase tileBase = tilemap.GetTile(position);
        if (tileBase != null && tileBase.Equals(unusableSpace)) {
            tilemap.SetTile(position, usableSpace);
        }
    }

    private void ReturnToHeap(Vector3Int position) {
        // Customer is using this space, penalty
        TileBase tileBase = tilemap.GetTile(position);
        if (tileBase != null && tileBase.Equals(OccupiedSpace)) {
            Customer customerToBeKicked = haveSpaceCustomers[position];
            haveSpaceCustomers.Remove(position);
            capital -= customerToBeKicked.spending * kickPenalty;
        }

        tilemap.SetTile(position, unusableSpace);
    }

    private void Free() {
        StopCoroutine("ReallocCoroutine");
        isReallocing = false;

        Realloc(-borrowedSpace, true);
    }

    private void Realloc(int deltaSize, bool speedup = false) {
        if (!isReallocing) {
            StartCoroutine(ReallocCoroutine(deltaSize, speedup));
        }
    }

    private IEnumerator ReallocCoroutine(int deltaSize, bool speedup) {
        isReallocing = true;
        int i = 0;
        int x = firstUnusablePosition.x;
        int y = firstUnusablePosition.y;
        Vector3Int location = new Vector3Int(x, y, 0); ;

        while (i < Mathf.Abs(deltaSize)) {
            //print(x.ToString() + ' ' + y.ToString());

            yield return new WaitForSeconds(allocTime * (speedup ? 0.1f : 1));

            if (deltaSize > 0) {
                BorrowFromHeap(location);
                borrowedSpace++;

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
                borrowedSpace--;
            }
        }

        firstUnusablePosition = location;
        isReallocing = false;
    }

    private IEnumerator ChargeSpaceCost() {
        while (true) {
            capital -= costPerSpacePerSecond * borrowedSpace;
            yield return new WaitForSeconds(1);
        }
    }

    private void PrintCapital() {
        capitalText.text = '$' + capital.ToString();
    }

    private void AssignSpace(Vector3Int position) {
        TileBase tileBase = tilemap.GetTile(position);
        if (tileBase != null && tileBase.Equals(usableSpace)) {
            Customer customer = queue[0];
            queue.RemoveAt(0);
            haveSpaceCustomers.Add(position, customer);

            tilemap.SetTile(position, OccupiedSpace);
            //print("assigned");
        }
    }

    // Visualise patient level of the customers
    private void VisualisePatient() {
        for (int i = 0; i < customerIcons.Length; i++) {
            if (i < queue.Count) {
                customerIcons[i].enabled = true;
                customerIcons[i].color = Color.Lerp(Color.white, Color.red, queue[i].timeInQueue / queue[i].patient);
            } else {
                customerIcons[i].enabled = false;
            }
        }
    }

    // increment time in queue. for those who run out of patient (time in queue > patient), leave
    private void Impatience() {
        List<Customer> isLeaving = new List<Customer>();
        foreach (Customer customer in queue) {
            customer.timeInQueue += Time.deltaTime;

            if (customer.timeInQueue >= customer.patient) {
                isLeaving.Add(customer);
            }
        }
        foreach (Customer customer in isLeaving) {
            star -= impatientPenalty;
            queue.Remove(customer);
        }
    }

    // for those who already have space, increment time that have space
    private void IncrementTimeHaveSpace() {
        List<KeyValuePair<Vector3Int, Customer>> isLeavingKvp = new List<KeyValuePair<Vector3Int, Customer>>();
        foreach (KeyValuePair<Vector3Int, Customer> kvp in haveSpaceCustomers) {
            kvp.Value.timeHaveSpace += Time.deltaTime;
            if (kvp.Value.timeHaveSpace >= kvp.Value.stayTime) {
                isLeavingKvp.Add(kvp);
            }
        }
        foreach (KeyValuePair<Vector3Int, Customer> kvp in isLeavingKvp) {
            capital += kvp.Value.spending;
            star += successRating;
            tilemap.SetTile(kvp.Key, usableSpace);
            haveSpaceCustomers.Remove(kvp.Key);
        }
    }

    // new customer lines up with a randomised period
    private IEnumerator RandomNewcomer() {
        while (true) {
            queue.Add(new Customer(Random.Range(patientRange.x, patientRange.y),
                Random.Range(spendingRange.x, spendingRange.y),
                Random.Range(stayTimeRange.x, stayTimeRange.y)));

            yield return new WaitForSeconds(Random.Range(timeBetweenNewcomerRange.x, timeBetweenNewcomerRange.y));
        }
    }

    private void ApplyEvent() {
        timeBetweenNewcomerRange = currentEvent.timeBetweenNewcomerRange;
        patientRange = currentEvent.patientRange;
        spendingRange = currentEvent.spendingRange;
        stayTimeRange = currentEvent.stayTimeRange;
    }

    private void EndOfDay() {
        StopAllCoroutines();
        operational = false;

        foreach (KeyValuePair<Vector3Int, Customer> kvp in haveSpaceCustomers) {
            tilemap.SetTile(kvp.Key, usableSpace);
        }
        haveSpaceCustomers.Clear();

        queue.Clear();

        Free();

        // trigger event
        //currentEvent = events[(int)(Random.value * events.Length)];
        currentEvent = events[1];
        ApplyEvent();

        // announce
        annoucer.text = "Another Day to feed.\n" + currentEvent.stringToBeAnnounced;
        annoucer.gameObject.SetActive(true);

        dayPassed = 0;
    }

    #endregion
}