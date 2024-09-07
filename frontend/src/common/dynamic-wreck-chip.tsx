import {Chip} from "@mui/material";

interface YesNoChipProps
{
    value: boolean;
}

export const DynamicWreckChip = (props: YesNoChipProps) => {

    let text = "No";
    let color: any = "info";
    let variant: any = "filled";
    if (props.value)
    {
        text = "Yes";
        color = "default";
        variant = "outlined";
    }

    return (
        <Chip size="small" color={color} label={text} variant={variant} />
    );
};