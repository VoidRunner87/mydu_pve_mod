import {Chip} from "@mui/material";

interface ActiveChipProps
{
    value: boolean;
}

export const ActiveChip = (props: ActiveChipProps) => {

    let text = "No";
    let color: any = "error";
    let variant: any = "outlined";
    if (props.value)
    {
        text = "Yes";
        color = "success";
        variant = "outlined";
    }

    return (
        <Chip size="small" color={color} label={text} variant={variant} />
    );
};