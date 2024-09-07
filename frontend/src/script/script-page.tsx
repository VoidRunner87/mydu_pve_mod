import React, {useEffect, useState} from "react";
import {getAll, ScriptItem} from "./script-service"
import {Button, Paper, Stack} from "@mui/material";
import {DataGrid, GridColDef} from "@mui/x-data-grid";
import DashboardContainer from "../dashboard/dashboard-container";
import {useNavigate} from "react-router-dom";

interface PrefabPageProps {}

const ScriptPage: React.FC<PrefabPageProps> = () => {

    const [data, setData] = useState<ScriptItem[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    const navigate = useNavigate();

    const fetchData = async () => {
        setLoading(true);
        const response = await getAll();
        setLoading(false);
        setData(response);
    };

    const columns: GridColDef[] = [
        {field: 'name', headerName: 'Name', width: 250, },
        {field: 'type', headerName: 'Type', valueGetter: value => value ?? 'composite'},
    ];

    const paginationModel = { page: 0, pageSize: 10 };

    useEffect(() => {
        fetchData();
    }, []);

    const handleAddClick = () => {
        navigate('create');
    };

    return (
        <DashboardContainer title="Scripts">
            <p>Scripts can perform many kinds of actions in the game - from spawning constructs to giving player titles or quanta</p>
            <Stack spacing={2} direction="row">
                <Button variant="contained" onClick={handleAddClick}>Add</Button>
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

export default ScriptPage;