import React from "react";
import {Box, Chip} from "@mui/material";

export interface TagsChipProps
{
    tags: string[];
}

export const TagsChip = (props: TagsChipProps) => {

    const tags = props.tags.map(r => (
        <Chip size="small" label={r}/>
    ));

    return (
        <Box>
            {tags}
        </Box>
    );
};