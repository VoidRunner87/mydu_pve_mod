import {
    GridRow,
    PlayerName,
    WidgetRow
} from "./widget";
import styled from "styled-components";
import {WidgetFlexButton} from "./buttons";

const PendingState = styled.div`
    display: flex;
    align-content: end;
`;

const InvitedActions = ({item, type}) => {
    if (type !== "invited") {
        return null;
    }

    return (
        <WidgetFlexButton onClick={() => window.modApi.cancelInvite(item.PlayerId)}>Cancel invite</WidgetFlexButton>
    )
};

const InvitedContainer = ({item, type}) => {
    if (type !== "invited") {
        return null;
    }

    return (
        <PendingState>Invited</PendingState>
    )
};

const PendingAcceptActions = ({item, type}) => {
    if (type !== "pending-accept") {
        return null;
    }

    return (
        <>
            <WidgetFlexButton onClick={() => window.modApi.acceptRequest(item.PlayerId)} className="positive">Accept</WidgetFlexButton>
            &nbsp;
            <WidgetFlexButton onClick={() => window.modApi.rejectRequest(item.PlayerId)}>Reject</WidgetFlexButton>
        </>
    )
};

const PendingAcceptContainer = ({item, type}) => {
    if (type !== "pending-accept") {
        return null;
    }

    return (
        <PendingState>Requested to join</PendingState>
    )
};

const PartyEntryPending = ({item, type}) => {

    return (
        <WidgetRow>
            <GridRow>
                <PlayerName>{item.PlayerName}</PlayerName>
                <InvitedContainer type={type} item={item}/>
                <PendingAcceptContainer type={type} item={item}/>
            </GridRow>
            <GridRow>
                <InvitedActions type={type} item={item}/>
                <PendingAcceptActions type={type} item={item}/>
            </GridRow>
        </WidgetRow>
    );
}

export default PartyEntryPending;