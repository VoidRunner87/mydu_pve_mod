import {ActionCardListContainer} from "./action-card-list-container";
import {SpawnActionCard} from "./spawn-action-card";
import {Alert, Box, Button} from "@mui/material";
import React from "react";
import {ActionCollection} from "../script-model";
import {DeleteActionCard} from "./delete-action-card";
import {RandomActionCard} from "./random-action-card";
import {GiveTitleActionCard} from "./give-title-action-card";

interface ActionListProps extends ActionCollection {

}

export const ActionList = (props: ActionListProps) => {

    const actionElements = props.actions.map(action => {
        switch (action.type){
            case "spawn":
                return <SpawnActionCard />
            case "delete":
                return <DeleteActionCard />
            case "random":
                return <RandomActionCard />
            case "give-title":
                return <GiveTitleActionCard />
            default:
                return null;
        }
    });

    const actionListElement = (actionElements.length > 0 ?
        <ActionCardListContainer>{actionElements}</ActionCardListContainer> :
        <Alert severity="info">Empty</Alert>
    );

    return (
        <>
            {actionListElement}
            <Box sx={{paddingTop: 2}}>
                <Button variant="outlined">Add Action</Button>
            </Box>
        </>
    );
}