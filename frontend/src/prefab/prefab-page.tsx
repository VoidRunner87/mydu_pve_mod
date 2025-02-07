import React, {useEffect, useState} from "react";
import {getAll, PrefabItem} from "./prefab-service"
import {Button, Paper, Stack} from "@mui/material";
import {DataGrid, GridColDef} from "@mui/x-data-grid";
import DashboardContainer from "../dashboard/dashboard-container";
import {DynamicWreckChip} from "../common/dynamic-wreck-chip";

interface PrefabPageProps {
}

const PrefabPage: React.FC<PrefabPageProps> = () => {

    const [data, setData] = useState<PrefabItem[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    const fetchData = async () => {
        setLoading(true);
        const response = await getAll();
        setLoading(false);
        setData(response);
    };

    const columns: GridColDef[] = [
        {field: 'name', headerName: 'Name', width: 250,},
        {field: 'folder', headerName: 'Folder'},
        {field: 'path', headerName: 'Blueprint', width: 250},
        {
            field: 'serverProperties', headerName: 'Wreck', width: 75,
            renderCell: params => <DynamicWreckChip value={params.value.isDynamicWreck}/>
        },
    ];

    const paginationModel = {page: 0, pageSize: 10};

    useEffect(() => {
        fetchData();
    }, []);

    return (
        <DashboardContainer title="Prefabs">
            <p>Blueprint and construct information to be used by scripts to spawn</p>
            <Stack spacing={2} direction="row">
                <Button variant="contained">Add</Button>
            </Stack>
            <br/>
            <Paper>
                <DataGrid
                    rows={data}
                    columns={columns}
                    initialState={{pagination: {paginationModel}}}
                    pageSizeOptions={[10, 20, 30, 40, 50, 100]}
                    checkboxSelection
                    sx={{border: 0}}
                />
            </Paper>
        </DashboardContainer>
    );
}

export default PrefabPage;