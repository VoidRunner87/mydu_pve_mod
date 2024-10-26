import styled from 'styled-components'

export const Container = styled.div`
    display: flex;
    justify-content: center;
    align-items: center;
    height: 100vh;
    z-index: 99999999 !important;
`;

export const Panel = styled.div`
    min-width: 1000px;
    width: 55vw;
    height: 75vh;
    background-color: rgb(25, 34, 41);
    border: 1px solid rgb(50, 79, 77);
    padding: 2px;
    z-index: 99999999 !important;
    display: flex;
    flex-direction: column;
`;

export const Header = styled.div`
    background-color: rgb(0, 0, 0);
    padding: 16px;
    display: flex;
    justify-content: space-between;
    align-items: center;
`;

export const Title = styled.span`
    color: white;
    font-size: 18px;
    margin-right: 10px;
`;

export const PanelCloseButton = styled.button`
    margin-left: auto;
    background-color: transparent;
    border: none;
    font-size: 40px;
    line-height: 0;
    cursor: pointer;
    padding: 0;
    color: white;

    line {
        stroke: white;
    }

    &:hover {
        line {
            stroke: rgb(250, 212, 122);
        }
    }
`;

export const PanelBody = styled.div`
    color: white;
    display: flex;
    justify-content: space-between;
    flex-grow: 1;
`;

export const SelectedCategoryButton = styled.button`
    background-color: rgb(250, 212, 122);
    padding: 16px;
    font-weight: bold;
    text-transform: uppercase;
    color: black;
    text-align: left;
    border: none;
    display: block;
    width: 100%;
    cursor: pointer;
`;

export const UnselectedCategoryButton = styled.button`
    display: block;
    background-color: rgb(27, 48, 56);
    padding: 16px;
    font-weight: bold;
    text-align: left;
    text-transform: uppercase;
    color: rgb(180, 221, 235);
    border: none;
    width: 100%;
    cursor: pointer;
`;

export const Tab = (props) => {
    return props.selected ? (<SelectedCategoryButton onClick={props.onClick}>{props.children}</SelectedCategoryButton>) :
        (<UnselectedCategoryButton onClick={props.onClick}>{props.children}</UnselectedCategoryButton>);
}

export const CloseButton = (props) => {
    return (
        <PanelCloseButton onClick={props.onClick}>
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="24" height="24">
                <line x1="4" y1="4" x2="20" y2="20" stroke="white" strokeWidth="2" strokeLinecap="round"/>
                <line x1="4" y1="20" x2="20" y2="4" stroke="white" strokeWidth="2" strokeLinecap="round"/>
            </svg>
        </PanelCloseButton>
    );
}