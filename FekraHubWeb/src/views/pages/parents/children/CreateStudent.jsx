'use client'

import React, { useEffect, useState } from 'react'

// MUI Imports
import Card from '@mui/material/Card'
import CardHeader from '@mui/material/CardHeader'
import CardContent from '@mui/material/CardContent'
import Grid from '@mui/material/Grid'
import TextField from '@mui/material/TextField'
import Select from '@mui/material/Select'
import FormControlLabel from '@mui/material/FormControlLabel'
import Button from '@mui/material/Button'
import Checkbox from '@mui/material/Checkbox'
import FormHelperText from '@mui/material/FormHelperText'
import MenuItem from '@mui/material/MenuItem'
import InputLabel from '@mui/material/InputLabel'
import FormControl from '@mui/material/FormControl'
import IconButton from '@mui/material/IconButton'
import Autocomplete from '@mui/material/Autocomplete'
import axios from 'axios'

// Third-party Imports
import { toast } from 'react-toastify'
import { useForm, Controller } from 'react-hook-form'

// Styled Component Imports
import AppReactDatepicker from '@/libs/styles/AppReactDatepicker'

const top100Films = [
  { id: '1', label: 'class1' },
  { id: '2', label: 'class2' },
  { id: '3', label: 'class3' },
  { id: '4', label: 'class4' },
  { id: '5', label: 'class5' },
  { id: '6', label: 'class6' },
  { id: '7', label: 'class7' }
]

const CreateStudent = () => {
  // States
  const [isPasswordShown, setIsPasswordShown] = useState(false)

  const [formData, setFormData] = useState({
    FirstName: '',
    LastName: '',
    Nationality: '',
    City: '',
    Note: '',
    Birthday: '03-04-2022',
    Street: '',
    StreetNr: '',
    ZipCode: '',
    ParentID: '0788b331-7b8e-4294-882e-560c884b4f8f',
    CourseID: '1'
  })

  const handleChange = e => {
    const { name, value } = e.target

    setFormData({
      ...formData,
      [name]: value
    })
  }

  // Hooks
  const {
    control,
    reset,
    formState: { errors }
  } = useForm({})

  const handleClickShowPassword = () => setIsPasswordShown(show => !show)

  const handleSubmit = async e => {
    e.preventDefault()

    const data = {
      FirstName: 'basel',
      LastName: 'wael',
      Birthday: '2010-01-01',
      Nationality: 'bbb',
      Note: 'bbbb',
      CourseID: '1',
      ParentID: '0788b331-7b8e-4294-882e-560c884b4f8f'
    }

    const formData = new FormData()

    for (const key in data) {
      formData.append(key, data[key])
    }

    try {
      const response = await axios.post('https://localhost:5008/api/Student', formData, {
        headers: {
          'Content-Type': 'multipart/form-data'
        }
      })
    } catch (error) {
      console.error('There was a problem with your fetch operation:', error)
    }
  }

  return (
    <div>
      <Card>
        <CardHeader title='Create Student' />
        <CardContent>
          <form onSubmit={handleSubmit}>
            <Grid container spacing={5}>
              <Grid item xs={12} sm={6}>
                <Controller
                  name='FirstName'
                  control={control}
                  rules={{ required: true }}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      fullWidth
                      label='First Name'
                      value={formData.FirstName}
                      placeholder='John'
                      onChange={handleChange}
                      {...(errors.FirstName && { error: true, helperText: 'This field is required.' })}
                    />
                  )}
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <Controller
                  name='LastName'
                  control={control}
                  rules={{ required: true }}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      fullWidth
                      value={formData.LastName}
                      onChange={handleChange}
                      label='Last Name'
                      placeholder='John'
                      {...(errors.LastName && { error: true, helperText: 'This field is required.' })}
                    />
                  )}
                />
              </Grid>

              <Grid item xs={12} sm={6}>
                <Controller
                  name='Birthday'
                  control={control}
                  rules={{ required: true }}
                  render={({ field: { value, onChange } }) => (
                    <AppReactDatepicker
                      selected={value}
                      showYearDropdown
                      showMonthDropdown
                      onChange={onChange}
                      placeholderText='MM/DD/YYYY'
                      customInput={
                        <TextField
                          value={value}
                          onChange={onChange}
                          fullWidth
                          label='Date Of Birth'
                          {...(errors.Birthday && { error: true, helperText: 'This field is required.' })}
                        />
                      }
                    />
                  )}
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <Controller
                  name='Street'
                  control={control}
                  rules={{ required: true }}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      fullWidth
                      value={formData.Street}
                      onChange={handleChange}
                      label='Street'
                      placeholder='Street And House Number'
                      {...(errors.lastName && { error: true, helperText: 'This field is required.' })}
                    />
                  )}
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <Controller
                  name='StreetNr'
                  control={control}
                  rules={{ required: true }}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      fullWidth
                      value={formData.StreetNr}
                      onChange={handleChange}
                      label='StreetNr'
                      placeholder='Street And House Number'
                      {...(errors.lastName && { error: true, helperText: 'This field is required.' })}
                    />
                  )}
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <Controller
                  name='ZipCode'
                  control={control}
                  rules={{ required: true }}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      fullWidth
                      value={formData.ZipCode}
                      onChange={handleChange}
                      label='ZipCode'
                      placeholder='ZipCode'
                      {...(errors.lastName && { error: true, helperText: 'This field is required.' })}
                    />
                  )}
                />
              </Grid>

              <Grid item xs={12} sm={6}>
                <Controller
                  name='City'
                  rules={{ required: true }}
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      fullWidth
                      label='City'
                      value={formData.City}
                      onChange={handleChange}
                      placeholder='City'
                      {...(errors.lastName && { error: true, helperText: 'This field is required.' })}
                    />
                  )}
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <Controller
                  name='Nationality'
                  control={control}
                  rules={{ required: true }}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      fullWidth
                      label='Nationality'
                      value={formData.Nationality}
                      onChange={handleChange}
                      placeholder='Nationality'
                      {...(errors.Nationality && { error: true, helperText: 'This field is required.' })}
                    />
                  )}
                />
              </Grid>

              <Grid item xs={12} sm={6}>
                <Controller
                  name='Note'
                  control={control}
                  rules={{ required: true }}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      rows={4}
                      cols={4}
                      fullWidth
                      value={formData.Note}
                      onChange={handleChange}
                      multiline
                      label='Notes'
                      {...(errors.Note && { error: true, helperText: 'This field is required.' })}
                    />
                  )}
                />
              </Grid>

              <Grid item xs={12} className='flex gap-4'>
                <Button variant='contained' endIcon={<i className='ri-send-plane-2-line' />} type='submit'>
                  Send
                </Button>
                <Button variant='outlined' type='reset' onClick={() => reset()}>
                  Cancel
                </Button>
              </Grid>
            </Grid>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}

export default CreateStudent
