import {Box, Chip} from "@mui/material";
import {Vector3} from "three";

interface SectorChipProps
{
    value: Vector3;
}

export const SectorChip = (props: SectorChipProps) => {

    const vec = new Vector3(
        props.value.x / 100000,
        props.value.y / 100000,
        props.value.z / 100000,
    );

    return (
        <Box>
            <Chip size="small" label={`${vec.x}, ${vec.y}, ${vec.z}`} />
        </Box>
    )
};