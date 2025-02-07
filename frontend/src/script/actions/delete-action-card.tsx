import {Alert} from "@mui/material";
import React from "react";
import {ActionCard} from "./action-card";

export const DeleteActionCard = () => {
    return (
        <ActionCard title="Delete Construct">
            <Alert severity="warning">This action will only succeed if the script context provides a
                ConstructId</Alert>
        </ActionCard>
    );
}