using System;
using UnityEngine;

public class GoalChaser : MonoBehaviour
{
	public GridManager gridManager;
	public GameObject goal;
	public float speed;

	private Pathfinding _pathfinder;

	private Path _path;
	private int _currWaypoint;

    void Start()
    {
		_pathfinder = gridManager.GetPathfinding();
		_path = _pathfinder.GetPath(transform.position, goal.transform.position);
		_currWaypoint = 1;
    }

    void Update()
    {
		if (_currWaypoint >= _path.PointCount) return;

		Vector2 target = _path.PointAt(_currWaypoint);
		transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

		if (GameMath.Vector2Compare(target, transform.position))
		{
			_currWaypoint++;
		}
    }
}
