using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Sex
{
    Male,
    Female
}

public enum CreatureAction
{
    GoingToFood,
    GoingToWater,
    GoingToMate,
}

public enum Direction
{
    Up,
    Right,
    Down,
    Left
}

public enum SpriteState
{
    Standing,
    Walking,
    Consuming
}

public struct Traits
{
    public int speed; // Update per speed ticks
    public int fov; // Degrees
    public float reproductiveUrge; // When to sacrifice essentials for mate
    public int desirability; // Visible males only
    public int gestationDuration; // Visible females only

    public Traits(int speed, int fov, float reproductiveUrge, int desirability, int gestationDuration)
    {
        this.speed = Mathf.Clamp(speed, 1, 30);
        this.fov = Mathf.Clamp(fov, 30, 150);
        this.reproductiveUrge = Mathf.Clamp01(reproductiveUrge);
        this.desirability = Mathf.Clamp(desirability, 1, 3);
        this.gestationDuration = gestationDuration; 
    }

    public static Traits Default => new Traits(5, 60, 0.4f, 2, 10);
}

public abstract class Animal : MonoBehaviour, IGameIterable
{
    // Cannot mutate
    public float hunger;
    public float thirst;
    public bool isAdult = true;
    public Direction facing;
    public int sightArea;
    protected Vector2Int currentTarget;
    public Queue<Animal> declined = new Queue<Animal>();
    public int mutationRate;
    public Sex sex;
    public bool waitingToBirth;
    public bool isGestating;

    // Sprites
    public Sprite[] female = new Sprite[3];
    public Sprite[] maleA1 = new Sprite[3];
    public Sprite[] maleA2 = new Sprite[3];
    public Sprite[] maleA3 = new Sprite[3];
    protected Sprite[] sprites;
    public SpriteRenderer animalSprite;
    public GameObject carcassPrefab;
    public GameObject mateParticlePrefab;

    [System.NonSerialized]
    public GameObject animalPrefab;

    // Can mutate
    public Traits traits;

    public WaveFunction map;
    public Transform animalContainer;
    public SimulationManager simulationManager;
    public Vector2Int pos;
    public CreatureAction currentAction;
    public Coroutine smoothMoveCoroutine;

    private int timeToDeathByHunger;
    private int timeToDeathByThirst;
    private int tickBorn;

    public Animal mother;
    public Traits mateTraits;

    public void TickUpdate(int currentTick)
    {
        if (waitingToBirth) GiveBirth();

        hunger += 1f / timeToDeathByHunger;
        thirst += 1f / timeToDeathByThirst;
        ChooseNextAction();

        if ((currentTick - tickBorn) % this.traits.speed == 0)
        {
            HandleInteractions();
        }
        if ((currentTick - tickBorn) % 60 == 0)
        {
            if (declined.Count > 0) declined.Dequeue();
            waitingToBirth = false;
        }

        if (hunger >= 1 || thirst >= 1)
        { // Dies
            Die();
        }
    }

    public void SetupAnimal(Vector2Int position, WaveFunction map, int tickBorn, Sex sex, SimulationManager simulationManager, GameObject animalPrefab)
    {
        this.traits = Traits.Default;
        this.hunger = 0f;
        this.thirst = 0f;
        this.facing = Direction.Left;
        this.waitingToBirth = false;
        this.declined.Clear();
        this.pos = position;
        this.map = map;
        this.tickBorn = tickBorn;
        this.sex = sex;
        this.simulationManager = simulationManager;
        this.animalPrefab = animalPrefab;
        this.mutationRate = TransportData.transportMutationRate;
        sightArea = 2 * map.dimensions;
        timeToDeathByHunger = 36 * Mathf.Max(1, this.traits.speed);
        timeToDeathByThirst = 36 * Mathf.Max(1, this.traits.speed);
        map.gridTile[Geometry.CoordinateToIndex(map.dimensions, pos)].occupant = this;

        SimulationManager.Instance.iterables.Add(this);
        GraphManager.animals.Add(this);

        if (this.mother != null)
        {
            this.isAdult = false;
            transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            SetTraits(mother);
        }

        SetSprites();
    }

    private void SetSprites()
    {
        if (this.sex == Sex.Female) sprites = female;
        else switch (traits.desirability)
        {
            case 1: sprites = maleA1; break;
            case 2: sprites = maleA2; break;
            case 3: sprites = maleA3; break;
        }

        animalSprite.sprite = sprites[(int) SpriteState.Standing];
    }

    protected void SetTraits(Animal mother)
    {
        // When born
        this.sex = (Sex) UnityEngine.Random.Range(0, 2);

        // Average each trait
        int childSpeed;
        int childFov;
        float childReproductiveUrge;
        int childDesirability;
        int childGestationDuration;
        float penalty = 2f - Mathf.Clamp01((float) mother.traits.gestationDuration / 10f);

        if (UnityEngine.Random.Range(1, 100) >= mutationRate)
        {
            // No mutations
            childSpeed = (mother.mateTraits.speed + mother.traits.speed) / 2;
            childFov = (mother.mateTraits.fov + mother.traits.fov) / 2;
            childReproductiveUrge = (mother.mateTraits.reproductiveUrge + mother.traits.reproductiveUrge) / 2f;
            childDesirability = (mother.mateTraits.desirability + mother.traits.desirability) / 2;
            childGestationDuration = (mother.mateTraits.gestationDuration + mother.traits.gestationDuration) / 2;
        }
        else
        {
            // Mutations
            childSpeed = (mother.mateTraits.speed + mother.traits.speed) / 2 + UnityEngine.Random.Range(-2, 3);
            childFov = (mother.mateTraits.fov + mother.traits.fov) / 2 + UnityEngine.Random.Range(-5, 6);
            childReproductiveUrge = (mother.mateTraits.reproductiveUrge + mother.traits.reproductiveUrge) / 2f + UnityEngine.Random.Range(-0.3f, 0.3f);
            childDesirability = (mother.mateTraits.desirability + mother.traits.desirability) / 2 + UnityEngine.Random.Range(-1, 2);
            childGestationDuration = (mother.mateTraits.gestationDuration + mother.traits.gestationDuration) / 2 + UnityEngine.Random.Range(-2, 3);
        }

        this.traits = new Traits(Mathf.RoundToInt(childSpeed * penalty), Mathf.RoundToInt(childFov * penalty), childReproductiveUrge, childDesirability, childGestationDuration);
        Traits adultTraits = new Traits(childSpeed, childFov, childReproductiveUrge, childDesirability, childGestationDuration);
        StartCoroutine(GrowUp(adultTraits));
    }

    IEnumerator GrowUp(Traits adultTraits)
    {
        float growUpTicks = 60f / Mathf.Max(0.1f, mother.traits.gestationDuration);
        yield return new WaitForSeconds(growUpTicks / simulationManager.tickRate);
        transform.localScale = Vector3.one;
        this.traits = adultTraits;
        this.isAdult = true;    
    }

    protected void ChooseNextAction()
    {
        if (thirst > this.traits.reproductiveUrge && thirst >= hunger)
        {
            currentAction = CreatureAction.GoingToWater;
            return;
        }

        if (hunger > this.traits.reproductiveUrge && hunger > thirst)
        {
            currentAction = CreatureAction.GoingToFood;
            return;
        }

        if (isAdult && !isGestating) currentAction = CreatureAction.GoingToMate;
        else currentAction = CreatureAction.GoingToWater;
    }

    protected void HandleInteractions()
    {
        Vector2Int target = Vector2Int.zero;
        if (pos == currentTarget) currentTarget = Vector2Int.zero;

        switch (currentAction)
        {
            case CreatureAction.GoingToFood:
                if (CanEat()) StartCoroutine(Consume());
                else target = SearchFood();
                break;

            case CreatureAction.GoingToWater:
                if (IsAdjacentToWater(pos)) StartCoroutine(Drink());
                else target = SearchWater();
                break;

            case CreatureAction.GoingToMate:
                Animal mate = GetMate(pos);
                if (mate != null) StartCoroutine(Mate(mate));
                else target = SearchMate();
                break;

        }

        if (target != Vector2Int.zero)
        {
            if (currentTarget != Vector2Int.zero && Heuristic(pos, target) > Heuristic(pos, currentTarget)) Move(Pathfind(currentTarget));
            else
            {
                currentTarget = target;
                Move(Pathfind(target));
            }
        }
    }

    protected abstract Vector2Int SearchFood(); // Return pos of food

    protected Vector2Int SearchWater() // Return pos of water
    {
        Vector2Int pointer = pos;
        int passes;
        Vector2Int[] sightTriangle = CalculateSightTriangle();

        switch (facing)
        {
            case Direction.Up:
                passes = sightTriangle[1].y - sightTriangle[0].y + 1;
                while (passes > 0)
                {
                    Vector2Int verticalReflectedPointer = new Vector2Int(2 * pos.x - pointer.x, pointer.y);
                    if (Geometry.IsInsideTriangle(sightTriangle, pointer) && Geometry.IsInsideGrid(map.dimensions, pointer))
                    {
                        if (IsAdjacentToWater(pointer)) return pointer;
                        if (Geometry.IsInsideGrid(map.dimensions, verticalReflectedPointer) && IsAdjacentToWater(verticalReflectedPointer)) return verticalReflectedPointer;
                        pointer.x++;
                    }
                    else
                    {
                        pointer.x = pos.x;
                        pointer.y++;
                        passes--;
                    }
                }
                break;

            case Direction.Right:
                passes = sightTriangle[1].x - sightTriangle[0].x + 1;
                while (passes > 0)
                {
                    Vector2Int horizontalReflectedPointer = new Vector2Int(pointer.x, 2 * pos.y - pointer.y);
                    if (Geometry.IsInsideTriangle(sightTriangle, pointer) && Geometry.IsInsideGrid(map.dimensions, pointer))
                    {
                        if (IsAdjacentToWater(pointer)) return pointer;
                        if (Geometry.IsInsideGrid(map.dimensions, horizontalReflectedPointer) && IsAdjacentToWater(horizontalReflectedPointer)) return horizontalReflectedPointer;
                        pointer.y++;
                    }
                    else
                    {
                        pointer.y = pos.y;
                        pointer.x++;
                        passes--;
                    }
                }
                break;

            case Direction.Down:
                passes = sightTriangle[0].y - sightTriangle[1].y + 1;
                while (passes > 0)
                {
                    Vector2Int verticalReflectedPointer = new Vector2Int(2 * pos.x - pointer.x, pointer.y);
                    if (Geometry.IsInsideTriangle(sightTriangle, pointer) && Geometry.IsInsideGrid(map.dimensions, pointer))
                    {
                        if (IsAdjacentToWater(pointer)) return pointer;
                        if (Geometry.IsInsideGrid(map.dimensions, verticalReflectedPointer) && IsAdjacentToWater(verticalReflectedPointer)) return verticalReflectedPointer;
                        pointer.x++;
                    }
                    else
                    {
                        pointer.x = pos.x;
                        pointer.y--;
                        passes--;
                    }
                }
                break;

            case Direction.Left:
                passes = sightTriangle[0].x - sightTriangle[1].x + 1;
                while (passes > 0)
                {
                    Vector2Int horizontalReflectedPointer = new Vector2Int(pointer.x, 2 * pos.y - pointer.y);
                    if (Geometry.IsInsideTriangle(sightTriangle, pointer) && Geometry.IsInsideGrid(map.dimensions, pointer))
                    {
                        if (IsAdjacentToWater(pointer)) return pointer;
                        if (Geometry.IsInsideGrid(map.dimensions, horizontalReflectedPointer) && IsAdjacentToWater(horizontalReflectedPointer)) return horizontalReflectedPointer;
                        pointer.y++;
                    }
                    else
                    {
                        pointer.y = pos.y;
                        pointer.x--;
                        passes--;
                    }
                }
                break;
        }

        if (currentTarget != Vector2Int.zero) return currentTarget;
        Vector2Int randomTile;
        int attempts = 0;

        do
        {
            randomTile = new Vector2Int(UnityEngine.Random.Range(0, map.dimensions), UnityEngine.Random.Range(0, map.dimensions));
            attempts++;
            if (attempts == 100) return pos;
        }
        while (!map.gridTile[Geometry.CoordinateToIndex(map.dimensions, randomTile)].isWalkable || map.gridTile[Geometry.CoordinateToIndex(map.dimensions, randomTile)].occupant != null);
        return randomTile;
    }

    protected Vector2Int SearchMate() // Return pos of mate
    {
        Vector2Int pointer = pos;
        int passes;
        Vector2Int[] sightTriangle = CalculateSightTriangle();

        switch (facing)
        {
            case Direction.Up:
                passes = sightTriangle[1].y - sightTriangle[0].y + 1;
                while (passes > 0)
                {
                    Vector2Int verticalReflectedPointer = new Vector2Int(2 * pos.x - pointer.x, pointer.y);
                    if (Geometry.IsInsideTriangle(sightTriangle, pointer) && Geometry.IsInsideGrid(map.dimensions, pointer))
                    {
                        if (GetMate(pointer) != null) return pointer;
                        if (Geometry.IsInsideGrid(map.dimensions, verticalReflectedPointer) && GetMate(verticalReflectedPointer) != null) return verticalReflectedPointer;

                        pointer.x++;
                    }
                    else
                    {
                        pointer.x = pos.x;
                        pointer.y++;
                        passes--;
                    }
                }
                break;

            case Direction.Right:
                passes = sightTriangle[1].x - sightTriangle[0].x + 1;
                while (passes > 0)
                {
                    Vector2Int horizontalReflectedPointer = new Vector2Int(pointer.x, 2 * pos.y - pointer.y);
                    if (Geometry.IsInsideTriangle(sightTriangle, pointer) && Geometry.IsInsideGrid(map.dimensions, pointer))
                    {
                        if (GetMate(pointer) != null) return pointer;
                        if (Geometry.IsInsideGrid(map.dimensions, horizontalReflectedPointer) && GetMate(horizontalReflectedPointer) != null) return horizontalReflectedPointer;

                        pointer.y++;
                    }
                    else
                    {
                        pointer.y = pos.y;
                        pointer.x++;
                        passes--;
                    }
                }
                break;

            case Direction.Down:
                passes = sightTriangle[0].y - sightTriangle[1].y + 1;
                while (passes > 0)
                {
                    Vector2Int verticalReflectedPointer = new Vector2Int(2 * pos.x - pointer.x, pointer.y);
                    if (Geometry.IsInsideTriangle(sightTriangle, pointer) && Geometry.IsInsideGrid(map.dimensions, pointer))
                    {
                        if (GetMate(pointer) != null) return pointer;
                        if (Geometry.IsInsideGrid(map.dimensions, verticalReflectedPointer) && GetMate(verticalReflectedPointer) != null) return verticalReflectedPointer;

                        pointer.x++;
                    }
                    else
                    {
                        pointer.x = pos.x;
                        pointer.y--;
                        passes--;
                    }
                }
                break;

            case Direction.Left:
                passes = sightTriangle[0].x - sightTriangle[1].x + 1;
                while (passes > 0)
                {
                    Vector2Int horizontalReflectedPointer = new Vector2Int(pointer.x, 2 * pos.y - pointer.y);
                    if (Geometry.IsInsideTriangle(sightTriangle, pointer) && Geometry.IsInsideGrid(map.dimensions, pointer))
                    {
                        if (GetMate(pointer) != null) return pointer;
                        if (Geometry.IsInsideGrid(map.dimensions, horizontalReflectedPointer) && GetMate(horizontalReflectedPointer) != null) return horizontalReflectedPointer;

                        pointer.y++;
                    }
                    else
                    {
                        pointer.y = pos.y;
                        pointer.x--;
                        passes--;
                    }
                }
                break;
        }

        if (currentTarget != Vector2Int.zero) return currentTarget;
        Vector2Int randomTile;

        int attempts = 0;

        do
        {
            randomTile = new Vector2Int(UnityEngine.Random.Range(0, map.dimensions), UnityEngine.Random.Range(0, map.dimensions));
            attempts++;
            if (attempts == 100) return pos;
        }
        while (!map.gridTile[Geometry.CoordinateToIndex(map.dimensions, randomTile)].isWalkable || map.gridTile[Geometry.CoordinateToIndex(map.dimensions, randomTile)].occupant != null);
        return randomTile;
    }

    protected Vector2Int Pathfind(Vector2Int target)
    {
        int size = map.dimensions * map.dimensions;
        int[] gScore = new int[size]; // Steps from start tile
        int[] cameFrom = new int[size]; // Previous tile of tile at index
        bool[] inOpen = new bool[size]; // To be explored
        bool[] explored = new bool[size]; // Explored

        for (int i = 0; i < size; i++)
        {
            gScore[i] = int.MaxValue;
            cameFrom[i] = -1;
        }

        int startIndex = Geometry.CoordinateToIndex(map.dimensions, pos);
        int targetIndex = Geometry.CoordinateToIndex(map.dimensions, target);

        gScore[startIndex] = 0;
        inOpen[startIndex] = true;
        List<int> open = new List<int> { startIndex };

        while (open.Count > 0)
        {
            // Linear search to find lowest distance to find what tile to explore next
            int current = open[0];
            Vector2Int currentPos = Geometry.IndexToCoordinate(map.dimensions, current);
            foreach (int tile in open)
            {
                Vector2Int tilePos = Geometry.IndexToCoordinate(map.dimensions, tile);
                if (gScore[tile] + Heuristic(tilePos, target) < gScore[current] + Heuristic(currentPos, target)) current = tile;
                currentPos = Geometry.IndexToCoordinate(map.dimensions, current);
            }

            if (current == targetIndex) break;

            // Exploring the tile found
            open.Remove(current);
            inOpen[current] = false;
            explored[current] = true;

            Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

            foreach (Vector2Int direction in directions)
            {
                Vector2Int neighbourPos = currentPos + direction;
                if (!Geometry.IsInsideGrid(map.dimensions, neighbourPos)) continue;
                int neighbourIndex = Geometry.CoordinateToIndex(map.dimensions, neighbourPos);
                if (!map.gridTile[neighbourIndex].isWalkable || explored[neighbourIndex] || map.gridTile[neighbourIndex].occupant != null) continue;

                int neighbourG = gScore[current] + 1;
                if (neighbourG < gScore[neighbourIndex])
                {
                    cameFrom[neighbourIndex] = current;
                    gScore[neighbourIndex] = neighbourG;
                    if (!inOpen[neighbourIndex])
                    {
                        open.Add(neighbourIndex);
                        inOpen[neighbourIndex] = true;
                    }
                }
            }
        }

        // Trace back to first step from start
        int step = targetIndex;
        while (cameFrom[step] != -1 && cameFrom[step] != startIndex)
        {
            step = cameFrom[step];
        }

        Vector2Int firstStep = Geometry.IndexToCoordinate(map.dimensions, step);
        float distanceTravelled = Mathf.Abs((firstStep - pos).x) + Mathf.Abs((firstStep - pos).y);

        if (cameFrom[step] == -1 || distanceTravelled != 1)
        {
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (Vector2Int direction in directions)
            {
                Vector2Int neighbourPos = pos + direction;
                if (!Geometry.IsInsideGrid(map.dimensions, neighbourPos)) continue; // Boundary check
                if (map.gridTile[Geometry.CoordinateToIndex(map.dimensions, neighbourPos)].isWalkable && map.gridTile[Geometry.CoordinateToIndex(map.dimensions, neighbourPos)].occupant == null) return direction;
            }
            return Vector2Int.zero;
        }

        return firstStep - pos;
    }

    private int Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    protected void Move(Vector2Int direction)
    {
        // Logic move
        map.gridTile[Geometry.CoordinateToIndex(map.dimensions, pos)].occupant = null;
        pos += direction;
        map.gridTile[Geometry.CoordinateToIndex(map.dimensions, pos)].occupant = this;

        // Animal Updates
        switch (direction)
        {
            case var d when d == Vector2Int.up:
                facing = Direction.Up;
                break;
            case var d when d == Vector2Int.right:
                facing = Direction.Right;
                animalSprite.flipX = true;
                break;
            case var d when d == Vector2Int.down:
                facing = Direction.Down;
                break;
            case var d when d == Vector2Int.left:
                facing = Direction.Left;
                animalSprite.flipX = false;
                break;
        }

        // Visual Move
        float offset = (map.dimensions - 1) / 2f;
        Vector3 targetPos = new Vector3(pos.x - offset, pos.y - offset, 0f);
        if (smoothMoveCoroutine != null)
        {
            StopCoroutine(smoothMoveCoroutine);
            transform.position = targetPos - new Vector3(direction.x, direction.y, 0f);
        }
        smoothMoveCoroutine = StartCoroutine(SmoothMove(targetPos));
    }

    protected IEnumerator SmoothMove(Vector3 target)
    {
        float speed = Vector3.Distance(transform.position, target) * simulationManager.tickRate;

        if (transform.position != target) animalSprite.sprite = sprites[(int) SpriteState.Walking];

        while (transform.position != target)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            yield return null;
        }

        animalSprite.sprite = sprites[(int) SpriteState.Standing];
        transform.position = target;
    }

    protected Vector2Int[] CalculateSightTriangle()
    {
        float angle = this.traits.fov * Mathf.PI / 180f;
        float halfTan = Mathf.Tan(angle / 2f);

        int sightDistance = Mathf.CeilToInt(Mathf.Sqrt(sightArea / halfTan));
        int halfWidth = Mathf.CeilToInt(sightDistance * halfTan);

        Vector2Int point1 = pos;
        Vector2Int point2;
        Vector2Int point3;

        switch (facing)
        {
            case Direction.Up:
                point2 = new Vector2Int(pos.x + halfWidth, pos.y + sightDistance);
                point3 = new Vector2Int(pos.x - halfWidth, pos.y + sightDistance);
                break;

            case Direction.Right:
                point2 = new Vector2Int(pos.x + sightDistance, pos.y + halfWidth);
                point3 = new Vector2Int(pos.x + sightDistance, pos.y - halfWidth);
                break;

            case Direction.Down:
                point2 = new Vector2Int(pos.x + halfWidth, pos.y - sightDistance);
                point3 = new Vector2Int(pos.x - halfWidth, pos.y - sightDistance);
                break;

            case Direction.Left:
                point2 = new Vector2Int(pos.x - sightDistance, pos.y + halfWidth);
                point3 = new Vector2Int(pos.x - sightDistance, pos.y - halfWidth);
                break;

            default:
                throw new System.ArgumentOutOfRangeException(nameof(facing), facing, "Invalid facing direction");
        }

        return new Vector2Int[] { point1, point2, point3 };
    }

    protected abstract IEnumerator Consume(); // Animation + Eat

    protected abstract bool CanEat(); // Detect if animal can eat on current tile

    private IEnumerator Drink()
    {
        animalSprite.sprite = sprites[(int) SpriteState.Consuming];

        yield return new WaitForSeconds(1f / simulationManager.tickRate);

        thirst = Mathf.Max(0f, thirst - 0.4f);
        animalSprite.sprite = sprites[(int) SpriteState.Standing];
    }

    private bool IsAdjacentToWater(Vector2Int pos)
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighbourPos = pos + direction;
            if (!Geometry.IsInsideGrid(map.dimensions, neighbourPos)) continue; // Boundary check
            if (map.gridTile[Geometry.CoordinateToIndex(map.dimensions, neighbourPos)].name == "Water(Clone)") return true;
        }
        return false;
    }

    private IEnumerator Mate(Animal mate)
    {
        yield return new WaitForSeconds(1f / simulationManager.tickRate);

        if (sex == Sex.Female && !IsRejected(mate) && !waitingToBirth)
        {
            mateTraits = mate.traits;

            GameObject mateParticle1 = Instantiate(mateParticlePrefab, transform.position, Quaternion.identity);
            ConfigureParticles(mateParticle1.GetComponent<ParticleSystem>());

            GameObject mateParticle2 = Instantiate(mateParticlePrefab, mate.transform.position, Quaternion.identity);
            ConfigureParticles(mateParticle2.GetComponent<ParticleSystem>());

            StartCoroutine(Gestate());
        }

        declined.Enqueue(mate);
    }

    private bool IsRejected(Animal mate)
    {
        int rejectChance = mate.traits.desirability * 25;

        return rejectChance < UnityEngine.Random.Range(0, 100);
    }

    private IEnumerator Gestate()
    {
        isGestating = true;
        yield return new WaitForSeconds(traits.gestationDuration / simulationManager.tickRate);
        GiveBirth();
    }

    private void GiveBirth()
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (Vector2Int direction in directions)
        {
            Vector2Int birthPos = pos + direction;

            if (!Geometry.IsInsideGrid(map.dimensions, birthPos)) continue; // Boundary check

            int index = Geometry.CoordinateToIndex(map.dimensions, birthPos);
            if (!map.gridTile[index].isWalkable || map.gridTile[index].occupant != null) continue; // Validity check

            float offset = (map.dimensions - 1) / 2f;
            Vector3 birthWorldPos = new Vector3(birthPos.x - offset, birthPos.y - offset, 0f);
            
            // Generating child
            GameObject childObject = Instantiate(animalPrefab, birthWorldPos, Quaternion.identity, animalContainer);
            Animal child = childObject.GetComponent<Animal>();
            child.mother = this;
            Sex sex = (Sex) UnityEngine.Random.Range(0, 2);
            child.animalContainer = animalContainer;
            child.SetupAnimal(birthPos, map, simulationManager.currentTick, sex, simulationManager, animalPrefab);
            isGestating = false;
            waitingToBirth = false;

            return;
        }

        waitingToBirth = true;
    }

    private Animal GetMate(Vector2Int pos)
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighbourPos = pos + direction;
            if (!Geometry.IsInsideGrid(map.dimensions, neighbourPos)) continue; // Boundary check

            Animal occupant = map.gridTile[Geometry.CoordinateToIndex(map.dimensions, neighbourPos)].occupant;

            if (occupant == null || occupant.GetType() != GetType() || occupant.sex == sex || declined.Contains(occupant) || occupant.currentAction != CreatureAction.GoingToMate || occupant.isGestating  || !occupant.isAdult) continue; // Valid mate check

            return occupant;
        }
        return null;
    }

    public void Die()
    {
        Tile tile = map.gridTile[Geometry.CoordinateToIndex(map.dimensions, pos)];

        StopAllCoroutines();
        simulationManager.iterables.Remove(this);
        GraphManager.animals.Remove(this);
        tile.occupant = null;
        GameObject carcass = Instantiate(carcassPrefab, transform.position, Quaternion.identity, tile.transform);
        carcass.GetComponent<Carcass>().simulationManager = simulationManager;
        Destroy(gameObject);
    }

    private void ConfigureParticles(ParticleSystem particleSystem)
    {
        float tickDuration = 1f / simulationManager.tickRate;
        var main = particleSystem.main;

        main.startLifetime = tickDuration;
        main.duration = tickDuration;

        particleSystem.Play();
    }
}
