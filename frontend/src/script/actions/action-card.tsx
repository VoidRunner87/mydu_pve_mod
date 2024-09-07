import {Box, Card, CardContent, Typography} from "@mui/material";
import React from "react";

interface ActionCardProps {
    children?: React.ReactNode;
    title: string;
}

export const ActionCard = (props: ActionCardProps) => {

    return (
        <Box sx={{p: 2, '& .MuiTextField-root': { marginBottom: 2 } }}>
            <Card variant="outlined" sx={{minWidth: 400}}>
                <CardContent>
                    <Typography gutterBottom sx={{color: 'text.secondary', marginBottom: 2}}>{props.title}</Typography>

                    {props.children}
                </CardContent>
            </Card>
        </Box>
    );
};