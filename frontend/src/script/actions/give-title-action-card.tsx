import {Alert, Box, Card, CardContent, FormGroup, TextField, Typography} from "@mui/material";
import React from "react";
import {ActionCard} from "./action-card";

export const GiveTitleActionCard = () => {
    return (
        <ActionCard title="Give Title">
            <Alert severity="warning">This action will only succeed if the script context provides
                PlayerIds</Alert>
            <FormGroup>
                <TextField fullWidth label="Title" variant="standard"/>
            </FormGroup>
        </ActionCard>
    );
}