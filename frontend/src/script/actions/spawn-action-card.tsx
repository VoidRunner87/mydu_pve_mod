import {
    Autocomplete,
    FormGroup,
    InputAdornment,
    TextField,
    Typography
} from "@mui/material";
import React, {useEffect, useState} from "react";
import {ActionCard} from "./action-card";
import {ActionList} from "./action-list";
import {getAll, PrefabItem} from "../../prefab/prefab-service"

export const SpawnActionCard = () => {

    const [loading, setLoading] = useState(true);
    const [prefabs, setPrefabs] = React.useState<PrefabItem[]>([]);

    const fetchPrefabListData = async () => {
        const result = await getAll();
        setPrefabs(result);
    }

    useEffect(() => {
        setLoading(true);
        fetchPrefabListData()
            .finally(() => setLoading(false));
    }, []);

    return (
        <ActionCard title="Spawn">
            <FormGroup>
                <Autocomplete
                    loading={loading}
                    disablePortal
                    options={prefabs}
                    getOptionKey={option => option.name}
                    getOptionLabel={option => option.name}
                    renderInput={(params) => <TextField {...params} label="Prefab"/>}
                />
                <TextField fullWidth required label="Min quantity to spawn"/>
                <TextField fullWidth required label="Max quantity to spawn"/>
                <TextField fullWidth required label="Random spawn sphere radius"
                           slotProps={{
                               input: {
                                   endAdornment: <InputAdornment position="end">meters</InputAdornment>
                               }
                           }}/>

                <TextField fullWidth label="Construct name"/>

                <Typography variant="body1">Events</Typography>
                <br />

                <Typography variant="body2">On Load</Typography>
                <ActionList actions={[]} />
                <br />
                <Typography variant="body2">On Sector Enter</Typography>
                <ActionList actions={[]} />
            </FormGroup>
        </ActionCard>
    );
}