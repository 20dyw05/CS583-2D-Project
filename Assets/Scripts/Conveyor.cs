using UnityEngine;

public class Conveyor : PlacedItem
{
    [SerializeField] float speed = 2f;
    [SerializeField] SurfaceEffector2D effector;
    [SerializeField] Transform arrow;

    protected override void OnPlaced() {
        if(Facing == Direction.Right) {
            effector.speed = speed;
            arrow.localRotation = Quaternion.Euler(0f, 0f, -90f);
        } else {
            effector.speed = -speed;
            arrow.localRotation = Quaternion.Euler(0f, 0f, 90f);
        }
    }
}