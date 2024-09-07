import {Box, Card} from "@mui/material";
import React from "react";

interface RootActionCardProps {
    children: React.ReactNode;
}

export const ActionCardListContainer = (props: RootActionCardProps) => {
    return (
        <Card variant="outlined" sx={{minWidth: 400}}>
            <Box sx={{p: 2, display: "flex", alignItems: "center", justifyContent: "center", flexDirection: "column"}}>
                {props.children}
            </Box>
        </Card>
    );
}

