import React, {useEffect, useState} from "react";
import {getAll, forceExpireAll, SectorInstanceItem} from "./sector-instance-service"
import {Button, Dialog, DialogActions, DialogContent, DialogTitle, Paper, Snackbar, Stack} from "@mui/material";
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
    const [snackbarOpen, setSnackbarOpen] = useState(false);
    const [snackbarMessage, setSnackbarMessage] = useState("Done");
    const [dialogOpen, setDialogOpen] = useState(false);
    const [dialogMessage, setDialogMessage] = useState("Are you sure?");
    const [dialogAction, setDialogAction] = useState("");

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

    function handleForceExpireAllClick() {
        setDialogMessage("Forcefully expiring all sectors will automatically regenerate them even if there are players around.");
        setDialogAction("expire-all");
        setDialogOpen(true);
    }

    function doForceExpireAll() {
        forceExpireAll()
            .then(() => {
                setSnackbarMessage("All sectors force expired");
                setSnackbarOpen(true);
            });
    }

    function handleSnackbarClose() {
        setSnackbarOpen(false);
    }

    function handleDialogClose() {
        setDialogOpen(false);
    }

    function doDialogAction()
    {
        switch (dialogAction)
        {
            case "expire-all":
                return doForceExpireAll();
        }

        setDialogAction("");
        setDialogOpen(false);
    }

    return (
        <DashboardContainer title="Sector Instances">
            <p>Sectors that have been procedurally generated and loaded</p>
            <Stack spacing={2} direction="row">
                <Button variant="contained" color="primary">Expire</Button>
                <Button onClick={handleForceExpireAllClick} variant="contained" color="error">Force Expire ALL</Button>
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
            <Snackbar
                open={snackbarOpen}
                autoHideDuration={3000}
                message={snackbarMessage}
                onClose={handleSnackbarClose}
                />
            <Dialog
                open={dialogOpen}
                onClose={handleDialogClose}
                >
                <DialogTitle>Are you sure?</DialogTitle>
                <DialogContent>{dialogMessage}</DialogContent>
                <DialogActions>
                    <Button onClick={handleDialogClose}>Cancel</Button>
                    <Button onClick={doDialogAction} autoFocus>Proceed</Button>
                </DialogActions>
            </Dialog>
        </DashboardContainer>
    );
}

export default SectorInstancePage;