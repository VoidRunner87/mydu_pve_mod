import React, {useEffect, useState} from "react";
import {getAll, SectorInstanceItem} from "./sector-instance-service"
import {Button, Paper, Stack} from "@mui/material";
import {DataGrid, GridColDef} from "@mui/x-data-grid";
import DashboardContainer from "../dashboard/dashboard-container";
import {VectorChip} from "../common/vector-chip";
import {DateChip} from "../common/date-chip";

interface PrefabPageProps {
}

const SectorInstancePage: React.FC<PrefabPageProps> = () => {

    const [data, setData] = useState<SectorInstanceItem[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    const fetchData = async () => {
        setLoading(true);
        const response = await getAll();
        setLoading(false);
        setData(response);
    };

    const columns: GridColDef[] = [
        {
            field: 'sector',
            headerName: 'Sector',
            minWidth: 250,
            renderCell: params => <VectorChip value={params.value}/>
        },
        {field: 'onLoadScript', headerName: 'On Load Script', minWidth: 200},
        {field: 'onSectorEnterScript', headerName: 'On Enter Script', minWidth: 200},
        {
            field: 'expiresAt',
            headerName: 'Expires at',
            minWidth: 175,
            renderCell: params => <DateChip value={params.value}/>
        },
        {
            field: 'forceExpiresAt',
            headerName: 'Force Expires at',
            minWidth: 175,
            renderCell: params => <DateChip value={params.value}/>
        },
    ];

    const paginationModel = {page: 0, pageSize: 10};

    useEffect(() => {
        fetchData();
    }, []);

    return (
        <DashboardContainer title="Sector Instances">
            <Stack spacing={2} direction="row">
                <Button variant="contained" color="primary">Expire</Button>
                <Button variant="contained" color="error">Force Expire</Button>
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

export default SectorInstancePage;