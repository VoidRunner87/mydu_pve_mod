import {Box, Card, CardContent, Typography} from "@mui/material";
import React from "react";
import {ActionList} from "./action-list";
import {ActionCard} from "./action-card";

export const RandomActionCard = () => {

    return (
        <ActionCard title="Random">
            <Typography variant="caption">Executes one of the actions at random</Typography>

            <ActionList actions={[]}/>
        </ActionCard>
    )
};