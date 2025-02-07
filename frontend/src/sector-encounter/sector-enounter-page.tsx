import React, {useEffect, useState} from "react";
import {getAll, SectorEncounterItem} from "./sector-encounter-service"
import {Button, Paper, Stack} from "@mui/material";
import {DataGrid, GridColDef} from "@mui/x-data-grid";
import DashboardContainer from "../dashboard/dashboard-container";
import {ActiveChip} from "../common/active-chip";
import {TagsChip} from "../common/tags-chip";

interface PrefabPageProps {
}

const SectorEncounterPage: React.FC<PrefabPageProps> = () => {

    const [data, setData] = useState<SectorEncounterItem[]>([]);
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
            field: 'name',
            headerName: 'Name',
            width: 200,
        },
        {field: 'onLoadScript', headerName: 'On Load Script', width: 180},
        {field: 'onSectorEnterScript', headerName: 'On Enter Script', width: 180},
        {
            field: 'active', headerName: 'Active', width: 180,
            renderCell: params => <ActiveChip value={params.value}/>
        },
        {field: 'properties', headerName: 'Tags', width: 180,
            renderCell: params => <TagsChip tags={params.value.tags} />},
    ];

    const paginationModel = {page: 0, pageSize: 10};

    useEffect(() => {
        fetchData();
    }, []);

    return (
        <DashboardContainer title="Sector Definitions">
            <p>Every time the mod needs to generate a new sector it will pick one of these active items at random and generate a sector instance</p>
            <Stack spacing={2} direction="row">
                <Button variant="contained" color="primary">Add</Button>
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

export default SectorEncounterPage;