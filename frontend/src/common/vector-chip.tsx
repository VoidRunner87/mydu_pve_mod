import {Vector} from "three/examples/jsm/physics/RapierPhysics";

interface VectorProps
{
    value: Vector;
}

export const VectorChip = (props: VectorProps) => {
    return (
        <span>{props.value.x},{props.value.y},{props.value.z}</span>
    )
};