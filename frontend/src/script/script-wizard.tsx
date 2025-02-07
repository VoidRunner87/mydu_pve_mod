import {
    FormGroup,
    Stack,
    TextField
} from "@mui/material";
import React, {useState} from "react";
import DashboardContainer from "../dashboard/dashboard-container";
import {ActionList} from "./actions/action-list";
import {CompositeActionModel} from "./script-model";

interface ScriptWizardProps {
    script?: CompositeActionModel;
}

const ScriptWizard = (props: ScriptWizardProps) => {

    const [script, setScript] =
        useState(props.script || new CompositeActionModel());

    function handleScriptNameChanged(event: React.ChangeEvent<HTMLInputElement>) {
        setScript({
            ...script,
            name: event.target.value
        });
    }

    return (
        <DashboardContainer title="New Script">
            <Stack spacing={2} direction="row">
                <FormGroup>
                    <TextField fullWidth required label="Script Name" value={script.name} onChange={handleScriptNameChanged}/>
                </FormGroup>
            </Stack>
            <br/>
            <ActionList actions={script.actions} />

        </DashboardContainer>
    );
};

export default ScriptWizard;