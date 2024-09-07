import React, {useEffect, useState} from "react";
import {getAll, ScriptItem} from "./script-service"
import {Button, Paper, Stack} from "@mui/material";
import {DataGrid, GridColDef} from "@mui/x-data-grid";
import DashboardContainer from "../dashboard/dashboard-container";

interface PrefabPageProps {}

const PrefabPage: React.FC<PrefabPageProps> = () => {

    const [data, setData] = useState<ScriptItem[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    const fetchData = async () => {
        setLoading(true);
        const response = await getAll();
        setLoading(false);
        setData(response);
    };

    const columns: GridColDef[] = [
        {field: 'name', headerName: 'Name', minWidth: 250, },
        {field: 'type', headerName: 'Type'},
        {field: 'prefab', headerName: 'Prefab', minWidth: 250},
    ];

    const paginationModel = { page: 0, pageSize: 10 };

    useEffect(() => {
        fetchData();
    }, []);

    return (
        <DashboardContainer title="Scripts">
            <Stack spacing={2} direction="row">
                <Button variant="contained">Add</Button>
            </Stack>
            <br />
            <Paper>
                <DataGrid
                    rows={data}
                    columns={columns}
                    getRowId={x => x.name}
                    initialState={{ pagination: { paginationModel } }}
                    pageSizeOptions={[10, 20, 30, 40, 50, 100]}
                    checkboxSelection
                    sx={{ border: 0 }}
                />
            </Paper>
        </DashboardContainer>
    );
}

export default PrefabPage;