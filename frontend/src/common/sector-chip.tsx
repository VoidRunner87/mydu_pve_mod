import {Box, Chip} from "@mui/material";
import {Vector3} from "three";

interface SectorChipProps
{
    value: Vector3;
}

export const SectorChip = (props: SectorChipProps) => {

    const vec = new Vector3(
        props.value.x / 1000000,
        props.value.y / 1000000,
        props.value.z / 1000000,
    );

    return (
        <Box>
            <Chip size="small" label={`${vec.x}, ${vec.y}, ${vec.z}`} />
        </Box>
    )
};