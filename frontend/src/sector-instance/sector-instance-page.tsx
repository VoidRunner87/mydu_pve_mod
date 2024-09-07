import React, {useEffect, useState} from "react";
import {getAll, SectorInstanceItem} from "./sector-instance-service"
import {Button, Paper, Stack} from "@mui/material";
import {DataGrid, GridColDef} from "@mui/x-data-grid";
import DashboardContainer from "../dashboard/dashboard-container";
import {SectorChip} from "../common/sector-chip";
import {DateChip, TimeSpanChip} from "../common/date-chip";

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
            width: 100,
            renderCell: params => <SectorChip value={params.value}/>
        },
        {field: 'onLoadScript', headerName: 'On Load Script', width: 180},
        {field: 'onSectorEnterScript', headerName: 'On Enter Script', width: 180},
        {
            field: 'expiresAt',
            headerName: 'Expires at',
            width: 275,
            renderCell: params => <>
                <DateChip value={params.value}/>
                <span>&nbsp;in&nbsp;</span>
                <TimeSpanChip value={params.value} now={new Date()}/>
            </>
        },
        {
            field: 'forceExpiresAt',
            headerName: 'Force Expires at',
            width: 175,
            renderCell: params => <DateChip value={params.value}/>
        },
    ];

    const paginationModel = {page: 0, pageSize: 10};

    useEffect(() => {
        fetchData();
    }, []);

    useEffect(() => {
        const intervalId = setInterval(() => {
           fetchData();
        }, 5000);

        return () => clearInterval(intervalId);
    }, []);

    return (
        <DashboardContainer title="Sector Instances">
            <p>Sectors that have been procedurally generated and loaded</p>
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